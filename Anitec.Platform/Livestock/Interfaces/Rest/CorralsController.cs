using System.Net.Mime;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Anitec.Platform.Livestock.Application.CommandServices;
using Anitec.Platform.Livestock.Application.QueryServices;
using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Domain.Model.Queries;
using Anitec.Platform.Livestock.Interfaces.Rest.Resources;
using Anitec.Platform.Livestock.Interfaces.Rest.Transform;
using Anitec.Platform.Shared.Application.Internal.Authorization;
using Anitec.Platform.Shared.Interfaces.Rest;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Livestock.Interfaces.Rest;

/// <summary>
/// REST API for corrals (pens) within a herd.
/// Readable by Rancher and Veterinarian; create/update/delete require Rancher.
/// Only the corrals of herds the user can see are returned or changed; anything else answers 403.
/// </summary>
[Authorize("Rancher", "Veterinarian")]
[ApiController]
[Route("api/v1/corrals")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Corrals endpoints")]
public class CorralsController(
    ICorralCommandService commandService,
    ICorralQueryService queryService,
    IAccessScopeService accessScopeService) : ControllerBase
{
    /// <summary>Lists all corrals.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        var result = await queryService.Handle(new GetAllCorralsQuery(), cancellationToken);
        return Ok(result.Where(corral => scope.CanSeeHerdId(corral.HerdId))
            .Select(CorralResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>Gets one corral by id.</summary>
    /// <param name="id">Corral primary key.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetCorralByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeHerdId(result.HerdId)) return this.Forbidden();
        return Ok(CorralResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    /// <summary>Creates a corral linked to a herd. Requires a non-empty name and valid HerdId.</summary>
    [Authorize("Rancher")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCorralResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Name)) return BadRequest(new { message = "Corral name is required." });
        if (resource.HerdId <= 0) return BadRequest(new { message = "HerdId must be greater than zero." });
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeHerdId(resource.HerdId)) return this.Forbidden();

        var command = CreateCorralCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return BadRequest(new { message = result.Message });
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            CorralResourceFromEntityAssembler.ToResourceFromEntity(result.Value));
    }

    /// <summary>Updates an existing corral by id. Rancher role only.</summary>
    [Authorize("Rancher")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateCorralResource resource, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetCorralByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeHerdId(existing.HerdId) || !scope.CanSeeHerdId(resource.HerdId)) return this.Forbidden();

        var command = UpdateCorralCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return NotFound();
        return Ok(CorralResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    /// <summary>Deletes a corral by id. Rancher role only.</summary>
    [Authorize("Rancher")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetCorralByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeHerdId(existing.HerdId)) return this.Forbidden();

        var result = await commandService.Handle(new DeleteCorralCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound();
        return NoContent();
    }
}
