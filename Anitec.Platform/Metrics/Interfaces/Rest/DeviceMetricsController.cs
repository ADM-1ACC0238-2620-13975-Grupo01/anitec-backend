using System.Net.Mime;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Anitec.Platform.Metrics.Application.CommandServices;
using Anitec.Platform.Metrics.Application.QueryServices;
using Anitec.Platform.Metrics.Domain.Model.Commands;
using Anitec.Platform.Metrics.Domain.Model.Queries;
using Anitec.Platform.Metrics.Interfaces.Rest.Resources;
using Anitec.Platform.Metrics.Interfaces.Rest.Transform;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Metrics.Interfaces.Rest;

/// <summary>
/// REST API for device metrics (sensor readings recorded from farm devices).
/// Available to Rancher and Veterinarian roles.
/// </summary>
[Authorize("Rancher", "Veterinarian")]
[ApiController]
[Route("api/v1/device-metrics")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available DeviceMetrics endpoints")]
public class DeviceMetricsController(
    IDeviceMetricCommandService commandService,
    IDeviceMetricQueryService queryService) : ControllerBase
{
    /// <summary>Lists all device metrics.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetAllDeviceMetricsQuery(), cancellationToken);
        return Ok(result.Select(DeviceMetricResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>Gets one device metric by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetDeviceMetricByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        return Ok(DeviceMetricResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    /// <summary>Creates a new device metric reading.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateDeviceMetricResource resource, CancellationToken cancellationToken)
    {
        var command = CreateDeviceMetricCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commandService.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            DeviceMetricResourceFromEntityAssembler.ToResourceFromEntity(result.Value));
    }

    /// <summary>Updates an existing device metric by id.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateDeviceMetricResource resource, CancellationToken cancellationToken)
    {
        var command = UpdateDeviceMetricCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return NotFound();
        return Ok(DeviceMetricResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    /// <summary>Deletes a device metric by id.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new DeleteDeviceMetricCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound();
        return NoContent();
    }
}
