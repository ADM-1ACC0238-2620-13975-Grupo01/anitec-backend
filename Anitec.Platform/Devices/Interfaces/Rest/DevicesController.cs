using System.Net.Mime;
using Anitec.Platform.Devices.Application.CommandServices;
using Anitec.Platform.Devices.Application.QueryServices;
using Anitec.Platform.Devices.Domain.Model.Commands;
using Anitec.Platform.Devices.Domain.Model.Queries;
using Anitec.Platform.Devices.Interfaces.Rest.Resources;
using Anitec.Platform.Devices.Interfaces.Rest.Transform;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Anitec.Platform.Metrics.Application.QueryServices;
using Anitec.Platform.Metrics.Domain.Model.Queries;
using Anitec.Platform.Metrics.Interfaces.Rest.Transform;
using Anitec.Platform.Shared.Application.Internal.Authorization;
using Anitec.Platform.Shared.Interfaces.Rest;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Devices.Interfaces.Rest;

/// <summary>
/// REST API for IoT/monitoring devices assigned to the farm.
/// Also exposes per-device metric listings (all metrics and latest metric).
/// Readable by Rancher and Veterinarian; mutations require Rancher.
/// A device is visible when it hangs from a herd or animal the user can see; anything else answers 403.
/// </summary>
[Authorize("Rancher", "Veterinarian")]
[ApiController]
[Route("api/v1/devices")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Devices endpoints")]
public class DevicesController(
    IDeviceCommandService commandService,
    IDeviceQueryService queryService,
    IDeviceMetricQueryService metricQueryService,
    IAccessScopeService accessScopeService) : ControllerBase
{
    /// <summary>Lists the devices the signed-in user can see.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        var result = await queryService.Handle(new GetAllDevicesQuery(), cancellationToken);
        return Ok(result.Where(scope.CanSeeDevice).Select(DeviceResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>Gets one device by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetDeviceByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeDevice(result)) return this.Forbidden();
        return Ok(DeviceResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    /// <summary>Registers a new device. Rancher role only.</summary>
    [Authorize("Rancher")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateDeviceResource resource, CancellationToken cancellationToken)
    {
        if (resource.HerdId is null && resource.AnimalId is null)
            return BadRequest(new { message = "A device must be assigned to a herd or an animal." });
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!CanAssign(scope, resource)) return this.Forbidden();

        var command = CreateDeviceCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commandService.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            DeviceResourceFromEntityAssembler.ToResourceFromEntity(result.Value));
    }

    /// <summary>Updates an existing device by id. Rancher role only.</summary>
    [Authorize("Rancher")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateDeviceResource resource, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetDeviceByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeDevice(existing) || !CanAssign(scope, resource)) return this.Forbidden();

        var command = UpdateDeviceCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return NotFound();
        return Ok(DeviceResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    /// <summary>Deletes a device by id. Rancher role only.</summary>
    [Authorize("Rancher")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetDeviceByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeDevice(existing)) return this.Forbidden();

        var result = await commandService.Handle(new DeleteDeviceCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Returns all metrics for a device, newest first.
    /// Returns 404 if the device does not exist and 403 if the user cannot see it.
    /// </summary>
    [HttpGet("{id:int}/metrics")]
    public async Task<IActionResult> GetMetricsByDevice(int id, CancellationToken cancellationToken)
    {
        var device = await queryService.Handle(new GetDeviceByIdQuery(id), cancellationToken);
        if (device is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeDevice(device)) return this.Forbidden();

        var metrics = await metricQueryService.Handle(new GetAllDeviceMetricsQuery(), cancellationToken);
        return Ok(metrics
            .Where(metric => metric.DeviceId == id)
            .OrderByDescending(metric => metric.RecordedAt)
            .Select(DeviceMetricResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>
    /// Returns the most recent metric for a device.
    /// Returns 404 if the device or any metric is missing and 403 if the user cannot see the device.
    /// </summary>
    [HttpGet("{id:int}/latest-metric")]
    public async Task<IActionResult> GetLatestMetricByDevice(int id, CancellationToken cancellationToken)
    {
        var device = await queryService.Handle(new GetDeviceByIdQuery(id), cancellationToken);
        if (device is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeDevice(device)) return this.Forbidden();

        var metrics = await metricQueryService.Handle(new GetAllDeviceMetricsQuery(), cancellationToken);
        var latest = metrics
            .Where(metric => metric.DeviceId == id)
            .OrderByDescending(metric => metric.RecordedAt)
            .FirstOrDefault();

        if (latest is null) return NotFound();

        return Ok(DeviceMetricResourceFromEntityAssembler.ToResourceFromEntity(latest));
    }

    /// <summary>True when every herd or animal the device is assigned to is visible to the user.</summary>
    private static bool CanAssign(AccessScope scope, CreateDeviceResource resource) =>
        (resource.HerdId is not { } herdId || scope.CanSeeHerdId(herdId))
        && (resource.AnimalId is not { } animalId || scope.CanSeeAnimalId(animalId));
}
