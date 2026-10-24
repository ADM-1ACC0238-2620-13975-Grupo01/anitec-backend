using System.Net.Mime;
using Anitec.Platform.Sanitary.Application.CommandServices;
using Anitec.Platform.Sanitary.Application.QueryServices;
using Anitec.Platform.Sanitary.Domain.Model.Commands;
using Anitec.Platform.Sanitary.Domain.Model.Queries;
using Anitec.Platform.Sanitary.Interfaces.Rest.Resources;
using Anitec.Platform.Sanitary.Interfaces.Rest.Transform;
using Anitec.Platform.Shared.Application.Internal.Authorization;
using Anitec.Platform.Shared.Interfaces.Rest;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Sanitary.Interfaces.Rest;

/// <summary>
/// REST API for sanitary/health events on animals (vaccinations, treatments, diagnoses, etc.).
/// Available to Rancher and Veterinarian roles, limited to the animals each user can see; anything else answers 403.
/// </summary>
[Authorize("Rancher", "Veterinarian")]
[ApiController]
[Route("api/v1/health-events")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available HealthEvents endpoints")]
public class HealthEventsController(
    IHealthEventCommandService commandService,
    IHealthEventQueryService queryService,
    IAccessScopeService accessScopeService) : ControllerBase
{
    /// <summary>Lists the health events of the animals the signed-in user can see.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        var result = await queryService.Handle(new GetAllHealthEventsQuery(), cancellationToken);
        return Ok(result.Where(healthEvent => scope.CanSeeAnimalId(healthEvent.AnimalId))
            .Select(HealthEventResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>Gets one health event by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetHealthEventByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeAnimalId(result.AnimalId)) return this.Forbidden();
        return Ok(HealthEventResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    /// <summary>Creates a new health event.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateHealthEventResource resource, CancellationToken cancellationToken)
    {
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeAnimalId(resource.AnimalId)) return this.Forbidden();

        var command = CreateHealthEventCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commandService.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            HealthEventResourceFromEntityAssembler.ToResourceFromEntity(result.Value));
    }

    /// <summary>Updates an existing health event by id.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateHealthEventResource resource, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetHealthEventByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeAnimalId(existing.AnimalId) || !scope.CanSeeAnimalId(resource.AnimalId)) return this.Forbidden();

        var command = UpdateHealthEventCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return NotFound();
        return Ok(HealthEventResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    /// <summary>Deletes a health event by id.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetHealthEventByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeAnimalId(existing.AnimalId)) return this.Forbidden();

        var result = await commandService.Handle(new DeleteHealthEventCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound();
        return NoContent();
    }
}
