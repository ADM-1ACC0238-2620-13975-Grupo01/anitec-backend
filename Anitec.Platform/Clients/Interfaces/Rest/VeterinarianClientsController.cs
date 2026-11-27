using System.Net.Mime;
using Anitec.Platform.Clients.Application.CommandServices;
using Anitec.Platform.Clients.Application.QueryServices;
using Anitec.Platform.Clients.Domain.Model.Entities;
using Anitec.Platform.Clients.Domain.Model.Commands;
using Anitec.Platform.Clients.Domain.Model.Queries;
using Anitec.Platform.Clients.Interfaces.Rest.Resources;
using Anitec.Platform.Iam.Application.QueryServices;
using Anitec.Platform.Iam.Domain.Model.Aggregates;
using Anitec.Platform.Iam.Domain.Model.Queries;
using Anitec.Platform.Iam.Infrastructure.Pipeline.Middleware.Attributes;
using Anitec.Platform.Shared.Interfaces.Rest;
using Anitec.Platform.Livestock.Application.QueryServices;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Model.Queries;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Clients.Interfaces.Rest;

/// <summary>
/// REST API for veterinarian–rancher client relationships.
/// Veterinarian-only: list clients, list available ranchers, ask a rancher for access, withdraw a link.
/// Asking creates a pending request; the rancher decides (see <see cref="RancherVeterinarianLinksController"/>)
/// and the veterinarian sees the data of a rancher only once the request is accepted.
/// Enriches responses with herd and animal counts per rancher.
/// A veterinarian can only manage their own client list; any other veterinarian id answers 403.
/// </summary>
[Authorize("Veterinarian")]
[ApiController]
[Route("api/v1/veterinarian")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Available Veterinarian Clients endpoints")]
public class VeterinarianClientsController(
    IVeterinarianClientCommandService commandService,
    IVeterinarianClientQueryService queryService,
    IUserQueryService userQueryService,
    IHerdQueryService herdQueryService,
    IAnimalQueryService animalQueryService) : ControllerBase
{
    /// <summary>Lists ranchers already linked as clients of the given veterinarian.</summary>
    [HttpGet("{veterinarianId:int}/clients")]
    public async Task<IActionResult> GetClients(int veterinarianId, CancellationToken cancellationToken)
    {
        if (veterinarianId != this.CurrentUser().Id) return this.Forbidden();

        var relationships = await queryService.Handle(
            new GetVeterinarianClientsByVeterinarianIdQuery(veterinarianId),
            cancellationToken);
        var users = await userQueryService.Handle(new GetAllUsersQuery(), cancellationToken);
        var herds = await herdQueryService.Handle(new GetAllHerdsQuery(), cancellationToken);
        var animals = await animalQueryService.Handle(new GetAllAnimalsQuery(), cancellationToken);

        var resources = relationships.Select(relationship =>
        {
            var rancher = users.FirstOrDefault(user => user.Id == relationship.RancherId);
            return ToClientResource(relationship, rancher, herds, animals);
        });

        return Ok(resources);
    }

    /// <summary>
    /// Lists ranchers this veterinarian can ask for access: those without a pending or accepted link,
    /// including herd and animal counts.
    /// </summary>
    [HttpGet("{veterinarianId:int}/available-ranchers")]
    public async Task<IActionResult> GetAvailableRanchers(int veterinarianId, CancellationToken cancellationToken)
    {
        if (veterinarianId != this.CurrentUser().Id) return this.Forbidden();

        var relationships = await queryService.Handle(
            new GetVeterinarianClientsByVeterinarianIdQuery(veterinarianId),
            cancellationToken);
        // A rancher with a pending or accepted link is not a candidate; one who rejected or revoked can be asked again.
        var assignedRancherIds = relationships
            .Where(client => client.IsPending || client.IsActive)
            .Select(client => client.RancherId)
            .ToHashSet();
        var users = await userQueryService.Handle(new GetAllUsersQuery(), cancellationToken);
        var herds = await herdQueryService.Handle(new GetAllHerdsQuery(), cancellationToken);
        var animals = await animalQueryService.Handle(new GetAllAnimalsQuery(), cancellationToken);

        var resources = users
            .Where(user => user.Role.Equals("Rancher", StringComparison.OrdinalIgnoreCase))
            .Where(user => !assignedRancherIds.Contains(user.Id))
            .Select(user =>
            {
                var rancherHerds = herds.Where(herd => herd.OwnerId == user.Id).ToList();
                var rancherHerdIds = rancherHerds.Select(herd => herd.Id).ToHashSet();
                return new AvailableRancherResource(
                    user.Id,
                    user.Username,
                    user.FullName,
                    rancherHerds.Count,
                    animals.Count(animal => rancherHerdIds.Contains(animal.HerdId)));
            });

        return Ok(resources);
    }

    /// <summary>
    /// Asks a rancher for access (US-020). The request is created as Pending and gives no access until the rancher
    /// accepts it. Answers 404 when the user is not a rancher and 409 when a request is already pending or the
    /// access is already active; a link that was rejected or revoked is reopened as a new pending request.
    /// </summary>
    [HttpPost("{veterinarianId:int}/clients/{rancherId:int}")]
    public async Task<IActionResult> AddClient(int veterinarianId, int rancherId, CancellationToken cancellationToken)
    {
        if (veterinarianId != this.CurrentUser().Id) return this.Forbidden();

        var rancher = await userQueryService.Handle(new GetUserByIdQuery(rancherId), cancellationToken);
        if (rancher is null || !rancher.Role.Equals("Rancher", StringComparison.OrdinalIgnoreCase))
            return NotFound(new { message = "Rancher not found." });

        var result = await commandService.Handle(
            new CreateVeterinarianClientCommand(veterinarianId, rancherId),
            cancellationToken);

        if (result.IsFailure) return Conflict(new { message = result.Message });

        var link = result.Value!;
        var resource = VeterinarianLinkAssembler.ToResource(link, [this.CurrentUser(), rancher]);
        return CreatedAtAction(nameof(GetClients), new { veterinarianId }, resource);
    }

    /// <summary>Withdraws a request or removes the veterinarian–rancher link, whatever its state.</summary>
    [HttpDelete("{veterinarianId:int}/clients/{rancherId:int}")]
    public async Task<IActionResult> RemoveClient(int veterinarianId, int rancherId, CancellationToken cancellationToken)
    {
        if (veterinarianId != this.CurrentUser().Id) return this.Forbidden();

        var result = await commandService.Handle(
            new DeleteVeterinarianClientCommand(veterinarianId, rancherId),
            cancellationToken);

        if (result.IsFailure) return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Maps a relationship entity plus related herds/animals into the API client resource
    /// (includes herd count and animal count for the rancher, zero until the link is accepted).
    /// </summary>
    private static VeterinarianClientResource ToClientResource(
        VeterinarianClient relationship,
        User? rancher,
        IEnumerable<Herd> herds,
        IEnumerable<Animal> animals)
    {
        // The totals are the rancher's data: they are only given once the rancher accepted the veterinarian.
        var rancherHerds = relationship.IsActive
            ? herds.Where(herd => herd.OwnerId == relationship.RancherId).ToList()
            : [];
        var rancherHerdIds = rancherHerds.Select(herd => herd.Id).ToHashSet();
        return new VeterinarianClientResource(
            relationship.Id,
            relationship.VeterinarianId,
            relationship.RancherId,
            rancher?.FullName ?? "Unknown rancher",
            relationship.Status,
            rancherHerds.Count,
            animals.Count(animal => rancherHerdIds.Contains(animal.HerdId)),
            relationship.RequestedAt,
            relationship.AcceptedAt);
    }
}
