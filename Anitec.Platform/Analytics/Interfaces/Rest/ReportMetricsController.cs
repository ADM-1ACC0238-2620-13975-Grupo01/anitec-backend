using System.Net.Mime;
using Anitec.Platform.Analytics.Application.CommandServices;
using Anitec.Platform.Analytics.Application.QueryServices;
using Anitec.Platform.Analytics.Domain.Model.Commands;
using Anitec.Platform.Analytics.Domain.Model.Queries;
using Anitec.Platform.Analytics.Interfaces.Rest.Resources;
using Anitec.Platform.Analytics.Interfaces.Rest.Transform;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Analytics.Interfaces.Rest;

[Authorize("Rancher", "Veterinarian")]
[ApiController]
[Route("api/v1/report-metrics")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available ReportMetrics endpoints")]
public class ReportMetricsController(
    IReportMetricCommandService commandService,
    IReportMetricQueryService queryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetAllReportMetricsQuery(), cancellationToken);
        return Ok(result.Select(ReportMetricResourceFromEntityAssembler.ToResourceFromEntity));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetReportMetricByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        return Ok(ReportMetricResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateReportMetricResource resource, CancellationToken cancellationToken)
    {
        var command = CreateReportMetricCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commandService.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            ReportMetricResourceFromEntityAssembler.ToResourceFromEntity(result.Value));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateReportMetricResource resource, CancellationToken cancellationToken)
    {
        var command = UpdateReportMetricCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return NotFound();
        return Ok(ReportMetricResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new DeleteReportMetricCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound();
        return NoContent();
    }
}
