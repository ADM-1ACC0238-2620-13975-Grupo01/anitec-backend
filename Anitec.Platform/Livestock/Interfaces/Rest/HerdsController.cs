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
/// REST API for herds (groups of animals under an owner).
/// Readable by Rancher and Veterinarian; mutations require Rancher.
/// Each user only gets the herds they own (rancher) or serve (veterinarian); anything else answers 403.
/// </summary>
[Authorize("Rancher", "Veterinarian")]
[ApiController]
[Route("api/v1/herds")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Herds endpoints")]
public class HerdsController(
    IHerdCommandService commandService,
    IHerdQueryService queryService,
    IAccessScopeService accessScopeService) : ControllerBase
{
    /// <summary>Lists the herds the signed-in user can see.</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        var result = await queryService.Handle(new GetAllHerdsQuery(), cancellationToken);
        return Ok(result.Where(scope.CanSeeHerd).Select(HerdResourceFromEntityAssembler.ToResourceFromEntity));
    }

    /// <summary>Gets one herd by id. Answers 403 when the herd is not visible to the user.</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await queryService.Handle(new GetHerdByIdQuery(id), cancellationToken);
        if (result is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeHerd(result)) return this.Forbidden();
        return Ok(HerdResourceFromEntityAssembler.ToResourceFromEntity(result));
    }

    /// <summary>
    /// Creates a herd. Requires name, location, owner, OwnerId, and main animal type.
    /// Rancher role only; the owner must be the signed-in rancher.
    /// </summary>
    [Authorize("Rancher")]
    [HttpPost]
    public async Task<IActionResult> Create(CreateHerdResource resource, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(resource.Name)) return BadRequest(new { message = "Herd name is required." });
        if (string.IsNullOrWhiteSpace(resource.Location)) return BadRequest(new { message = "Herd location is required." });
        if (string.IsNullOrWhiteSpace(resource.Owner)) return BadRequest(new { message = "Herd owner is required." });
        if (resource.OwnerId <= 0) return BadRequest(new { message = "OwnerId must be greater than zero." });
        if (string.IsNullOrWhiteSpace(resource.MainType)) return BadRequest(new { message = "Main type is required." });
        if (resource.OwnerId != this.CurrentUser().Id) return this.Forbidden();

        var command = CreateHerdCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return BadRequest(new { message = result.Message });
        return CreatedAtAction(nameof(GetById), new { id = result.Value!.Id },
            HerdResourceFromEntityAssembler.ToResourceFromEntity(result.Value));
    }

    /// <summary>Updates an existing herd by id. Rancher role only.</summary>
    [Authorize("Rancher")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CreateHerdResource resource, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetHerdByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeHerd(existing) || resource.OwnerId != this.CurrentUser().Id) return this.Forbidden();

        var command = UpdateHerdCommandFromResourceAssembler.ToCommandFromResource(id, resource);
        var result = await commandService.Handle(command, cancellationToken);
        if (result.IsFailure) return NotFound();
        return Ok(HerdResourceFromEntityAssembler.ToResourceFromEntity(result.Value!));
    }

    /// <summary>Deletes a herd by id. Rancher role only.</summary>
    [Authorize("Rancher")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var existing = await queryService.Handle(new GetHerdByIdQuery(id), cancellationToken);
        if (existing is null) return NotFound();
        var scope = await this.ScopeAsync(accessScopeService, cancellationToken);
        if (!scope.CanSeeHerd(existing)) return this.Forbidden();

        var result = await commandService.Handle(new DeleteHerdCommand(id), cancellationToken);
        if (result.IsFailure) return NotFound();
        return NoContent();
    }
}
