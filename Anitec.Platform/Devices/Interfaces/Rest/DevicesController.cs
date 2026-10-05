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
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Devices.Interfaces.Rest;

/// <summary>
/// REST API for IoT/monitoring devices assigned to the farm.
/// Also exposes per-device metric listings (all metrics and latest metric).
/// Readable by Rancher and Veterinarian; mutations require Rancher.
/// </summary>
[Authorize("Rancher", "Veterinarian")]
[ApiController]
[Route("api/v1/devices")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Devices endpoints")]
public class DevicesController(
    IDeviceCommandService commandService,
    IDeviceQueryService queryService,
    IDeviceMetricQueryService metricQueryService) : ControllerBase
{
    /// <summary>Lists all registered devices.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetAllDevicesQuery(), cancellationToken);
        return Ok(result.Select(DeviceResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>Gets one device by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetDeviceByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        return Ok(DeviceResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    /// <summary>Registers a new device. Rancher role only.</summary>
    [Authorize("Rancher")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateDeviceResource resource, CancellationToken cancellationToken)
    {
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
        var result = await commandService.Handle(new DeleteDeviceCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound();
        return NoContent();
    }

    /// <summary>
    /// Returns all metrics for a device, newest first.
    /// Returns 404 if the device does not exist.
    /// </summary>
    [HttpGet("{id:int}/metrics")]
    public async Task<IActionResult> GetMetricsByDevice(int id, CancellationToken cancellationToken)
    {
        var device = await queryService.Handle(new GetDeviceByIdQuery(id), cancellationToken);
        if (device is null) return NotFound();

        var metrics = await metricQueryService.Handle(new GetAllDeviceMetricsQuery(), cancellationToken);
        return Ok(metrics
            .Where(metric => metric.DeviceId == id)
            .OrderByDescending(metric => metric.RecordedAt)
            .Select(DeviceMetricResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>
    /// Returns the most recent metric for a device.
    /// Returns 404 if the device or any metric is missing.
    /// </summary>
    [HttpGet("{id:int}/latest-metric")]
    public async Task<IActionResult> GetLatestMetricByDevice(int id, CancellationToken cancellationToken)
    {
        var device = await queryService.Handle(new GetDeviceByIdQuery(id), cancellationToken);
        if (device is null) return NotFound();

        var metrics = await metricQueryService.Handle(new GetAllDeviceMetricsQuery(), cancellationToken);
        var latest = metrics
            .Where(metric => metric.DeviceId == id)
            .OrderByDescending(metric => metric.RecordedAt)
            .FirstOrDefault();

        if (latest is null) return NotFound();

        return Ok(DeviceMetricResourceFromEntityAssembler.ToResourceFromEntity(latest));
    }
}
