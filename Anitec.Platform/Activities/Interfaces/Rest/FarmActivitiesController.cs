using System.Net.Mime;
using Anitec.Platform.Activities.Application.CommandServices;
using Anitec.Platform.Activities.Application.QueryServices;
using Anitec.Platform.Activities.Domain.Model.Commands;
using Anitec.Platform.Activities.Domain.Model.Queries;
using Anitec.Platform.Activities.Interfaces.Rest.Resources;
using Anitec.Platform.Activities.Interfaces.Rest.Transform;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Anitec.Platform.Shared.Application.Internal.Authorization;
using Anitec.Platform.Shared.Interfaces.Rest;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Activities.Interfaces.Rest;

/// <summary>
/// REST API for farm activities/events (scheduled or logged work on the farm).
/// Route is api/v1/farm-events. Available to Rancher and Veterinarian.
/// A rancher gets the activities about them; a veterinarian gets the ones they created and those about their clients.
/// Anything else answers 403.
/// </summary>
[Authorize("Rancher", "Veterinarian")]
[ApiController]
[Route("api/v1/farm-events")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available FarmActivities endpoints")]
public class FarmActivitiesController(
    IFarmActivityCommandService commandService,
    IFarmActivityQueryService queryService,
    IAccessScopeService accessScopeService) : ControllerBase
{
    /// <summary>Lists the farm activities the signed-in user can see.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        var result = await queryService.Handle(new GetAllFarmActivitiesQuery(), cancellationToken);
        return Ok(result.Where(scope.CanSeeActivity)
            .Select(FarmActivityResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>Gets one farm activity by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetFarmActivityByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeActivity(result)) return this.Forbidden();
        return Ok(FarmActivityResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    /// <summary>Creates a new farm activity.</summary>
    [HttpPost]
    public async Task<IActionResult> Create(CreateFarmActivityResource resource, CancellationToken cancellationToken)
    {
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanWriteActivity(resource.OwnerId, resource.VeterinarianId)) return this.Forbidden();

        var command = CreateFarmActivityCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commandService.Handle(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            FarmActivityResourceFromEntityAssembler.ToResourceFromEntity(result.Value));
    }

    /// <summary>Updates an existing farm activity by id.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateFarmActivityResource resource, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetFarmActivityByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeActivity(existing) || !scope.CanWriteActivity(resource.OwnerId, resource.VeterinarianId))
            return this.Forbidden();

        var command = UpdateFarmActivityCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return NotFound();
        return Ok(FarmActivityResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    /// <summary>Deletes a farm activity by id.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetFarmActivityByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeActivity(existing)) return this.Forbidden();

        var result = await commandService.Handle(new DeleteFarmActivityCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound();
        return NoContent();
    }
}
