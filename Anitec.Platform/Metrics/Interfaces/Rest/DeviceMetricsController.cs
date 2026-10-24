using System.Net.Mime;
using Anitec.Platform.Devices.Application.QueryServices;
using Anitec.Platform.Devices.Domain.Model.Queries;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Anitec.Platform.Metrics.Application.CommandServices;
using Anitec.Platform.Metrics.Application.QueryServices;
using Anitec.Platform.Metrics.Domain.Model.Commands;
using Anitec.Platform.Metrics.Domain.Model.Queries;
using Anitec.Platform.Metrics.Interfaces.Rest.Resources;
using Anitec.Platform.Metrics.Interfaces.Rest.Transform;
using Anitec.Platform.Shared.Application.Internal.Authorization;
using Anitec.Platform.Shared.Interfaces.Rest;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Metrics.Interfaces.Rest;

/// <summary>
/// REST API for device metrics (sensor readings recorded from farm devices).
/// Available to Rancher and Veterinarian roles, limited to the devices each user can see; anything else answers 403.
/// </summary>
[Authorize("Rancher", "Veterinarian")]
[ApiController]
[Route("api/v1/device-metrics")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available DeviceMetrics endpoints")]
public class DeviceMetricsController(
    IDeviceMetricCommandService commandService,
    IDeviceMetricQueryService queryService,
    IDeviceQueryService deviceQueryService,
    IAccessScopeService accessScopeService) : ControllerBase
{
    /// <summary>Lists the metrics of the devices the signed-in user can see.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var visibleDeviceIds = await VisibleDeviceIdsAsync(cancellationToken);
        var result = await queryService.Handle(new GetAllDeviceMetricsQuery(), cancellationToken);
        return Ok(result.Where(metric => visibleDeviceIds.Contains(metric.DeviceId))
            .Select(DeviceMetricResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>Gets one device metric by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetDeviceMetricByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        if (!(await VisibleDeviceIdsAsync(cancellationToken)).Contains(result.DeviceId)) return this.Forbidden();
        return Ok(DeviceMetricResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    /// <summary>Creates a new device metric reading.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateDeviceMetricResource resource, CancellationToken cancellationToken)
    {
        if (!(await VisibleDeviceIdsAsync(cancellationToken)).Contains(resource.DeviceId)) return this.Forbidden();

        var command = CreateDeviceMetricCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commandService.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            DeviceMetricResourceFromEntityAssembler.ToResourceFromEntity(result.Value));
    }

    /// <summary>Updates an existing device metric by id.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateDeviceMetricResource resource, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetDeviceMetricByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var visibleDeviceIds = await VisibleDeviceIdsAsync(cancellationToken);
        if (!visibleDeviceIds.Contains(existing.DeviceId) || !visibleDeviceIds.Contains(resource.DeviceId))
            return this.Forbidden();

        var command = UpdateDeviceMetricCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return NotFound();
        return Ok(DeviceMetricResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    /// <summary>Deletes a device metric by id.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetDeviceMetricByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        if (!(await VisibleDeviceIdsAsync(cancellationToken)).Contains(existing.DeviceId)) return this.Forbidden();

        var result = await commandService.Handle(new DeleteDeviceMetricCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound();
        return NoContent();
    }

    /// <summary>Ids of the devices that hang from a herd or animal the signed-in user can see.</summary>
    private async Task<HashSet<int>> VisibleDeviceIdsAsync(CancellationToken cancellationToken)
    {
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        var devices = await deviceQueryService.Handle(new GetAllDevicesQuery(), cancellationToken);
        return devices.Where(scope.CanSeeDevice).Select(device => device.Id).ToHashSet();
    }
}
