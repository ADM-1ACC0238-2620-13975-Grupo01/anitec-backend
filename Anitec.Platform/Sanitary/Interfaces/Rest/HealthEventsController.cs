using System.Net.Mime;
using Anitec.Platform.Sanitary.Application.CommandServices;
using Anitec.Platform.Sanitary.Application.QueryServices;
using Anitec.Platform.Sanitary.Domain.Model.Commands;
using Anitec.Platform.Sanitary.Domain.Model.Queries;
using Anitec.Platform.Sanitary.Interfaces.Rest.Resources;
using Anitec.Platform.Sanitary.Interfaces.Rest.Transform;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Sanitary.Interfaces.Rest;

/// <summary>
/// REST API for sanitary/health events on animals (vaccinations, treatments, diagnoses, etc.).
/// Available to Rancher and Veterinarian roles.
/// </summary>
[Authorize("Rancher", "Veterinarian")]
[ApiController]
[Route("api/v1/health-events")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available HealthEvents endpoints")]
public class HealthEventsController(
    IHealthEventCommandService commandService,
    IHealthEventQueryService queryService) : ControllerBase
{
    /// <summary>Lists all health events.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetAllHealthEventsQuery(), cancellationToken);
        return Ok(result.Select(HealthEventResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>Gets one health event by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetHealthEventByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        return Ok(HealthEventResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    /// <summary>Creates a new health event.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateHealthEventResource resource, CancellationToken cancellationToken)
    {
        var command = CreateHealthEventCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commandService.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            HealthEventResourceFromEntityAssembler.ToResourceFromEntity(result.Value));
    }

    /// <summary>Updates an existing health event by id.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateHealthEventResource resource, CancellationToken cancellationToken)
    {
        var command = UpdateHealthEventCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return NotFound();
        return Ok(HealthEventResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    /// <summary>Deletes a health event by id.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new DeleteHealthEventCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound();
        return NoContent();
    }
}
