using System.Net.Mime;
using Anitec.Platform.Clients.Application.CommandServices;
using Anitec.Platform.Clients.Application.QueryServices;
using Anitec.Platform.Clients.Domain.Model;
using Anitec.Platform.Clients.Domain.Model.Commands;
using Anitec.Platform.Clients.Domain.Model.Entities;
using Anitec.Platform.Clients.Domain.Model.Queries;
using Anitec.Platform.Clients.Interfaces.Rest.Resources;
using Anitec.Platform.Iam.Application.QueryServices;
using Anitec.Platform.Iam.Domain.Model.Queries;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Interfaces.Rest;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Clients.Interfaces.Rest;

/// <summary>
/// The rancher's side of the consent flow: see the requests of veterinarians (US-020), accept or reject them
/// (US-021) and revoke an accepted one (US-022). A rancher only manages the links addressed to them; any
/// other rancher id answers 403.
/// </summary>
[Authorize("Rancher")]
[ApiController]
[Route("api/v1/rancher/{rancherId:int}/veterinarian-links")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Consent of veterinarians to access the data of a rancher")]
public class RancherVeterinarianLinksController(
    IVeterinarianClientCommandService commandService,
    IVeterinarianClientQueryService queryService,
    IUserQueryService userQueryService) : ControllerBase
{
    /// <summary>
    /// Lists the veterinarians who asked for access or have it, newest request first. Pass <c>status</c>
    /// (Pending, Accepted, Rejected or Revoked) to keep only those in that state.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetLinks(int rancherId, [FromQuery] string? status, CancellationToken cancellationToken)
    {
        if (rancherId != this.CurrentUser().Id) return this.Forbidden();

        var links = (await queryService.Handle(new GetVeterinarianLinksByRancherIdQuery(rancherId, status), cancellationToken))
            .ToList();
        var users = (await userQueryService.Handle(new GetAllUsersQuery(), cancellationToken)).ToList();

        return Ok(links.Select(link => VeterinarianLinkAssembler.ToResource(link, users)));
    }

    /// <summary>Accepts a pending request: the veterinarian can now see the data of the rancher.</summary>
    [HttpPost("{linkId:int}/accept")]
    public async Task<IActionResult> Accept(int rancherId, int linkId, CancellationToken cancellationToken)
    {
        if (rancherId != this.CurrentUser().Id) return this.Forbidden();

        var result = await commandService.Handle(new AcceptVeterinarianRequestCommand(linkId, rancherId), cancellationToken);
        return await RespondAsync(result, cancellationToken);
    }

    /// <summary>Rejects a pending request: the veterinarian gets no access.</summary>
    [HttpPost("{linkId:int}/reject")]
    public async Task<IActionResult> Reject(int rancherId, int linkId, CancellationToken cancellationToken)
    {
        if (rancherId != this.CurrentUser().Id) return this.Forbidden();

        var result = await commandService.Handle(new RejectVeterinarianRequestCommand(linkId, rancherId), cancellationToken);
        return await RespondAsync(result, cancellationToken);
    }

    /// <summary>
    /// Revokes the access of a veterinarian. The veterinarian can no longer see or register anything for this
    /// rancher; what they registered before keeps its author. Answers 404 when there is no active access.
    /// </summary>
    [HttpPost("{linkId:int}/revoke")]
    public async Task<IActionResult> Revoke(int rancherId, int linkId, CancellationToken cancellationToken)
    {
        if (rancherId != this.CurrentUser().Id) return this.Forbidden();

        var result = await commandService.Handle(new RevokeVeterinarianAccessCommand(linkId, rancherId), cancellationToken);
        return await RespondAsync(result, cancellationToken);
    }

    private async Task<IActionResult> RespondAsync(Result<VeterinarianClient> result, CancellationToken cancellationToken)
    {
        if (result.IsSuccess)
        {
            var users = (await userQueryService.Handle(new GetAllUsersQuery(), cancellationToken)).ToList();
            return Ok(VeterinarianLinkAssembler.ToResource(result.Value!, users));
        }

        return result.Error switch
        {
            ClientsError.VeterinarianClientNotFound or ClientsError.NoActiveAccess => NotFound(new { message = result.Message }),
            ClientsError.RequestNotPending => Conflict(new { message = result.Message }),
            _ => BadRequest(new { message = result.Message })
        };
    }
}
