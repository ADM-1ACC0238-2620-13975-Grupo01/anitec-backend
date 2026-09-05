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
using Anitec.Platform.Livestock.Application.QueryServices;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Model.Queries;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Anitec.Platform.Clients.Interfaces.Rest;

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
    [HttpGet("{veterinarianId:int}/clients")]
    public async Task<IActionResult> GetClients(int veterinarianId, CancellationToken cancellationToken)
    {
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

    [HttpGet("{veterinarianId:int}/available-ranchers")]
    public async Task<IActionResult> GetAvailableRanchers(int veterinarianId, CancellationToken cancellationToken)
    {
        var relationships = await queryService.Handle(
            new GetVeterinarianClientsByVeterinarianIdQuery(veterinarianId),
            cancellationToken);
        var assignedRancherIds = relationships.Select(client => client.RancherId).ToHashSet();
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

    [HttpPost("{veterinarianId:int}/clients/{rancherId:int}")]
    public async Task<IActionResult> AddClient(int veterinarianId, int rancherId, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(
            new CreateVeterinarianClientCommand(veterinarianId, rancherId),
            cancellationToken);

        if (result.IsFailure) return Conflict(new { message = result.Message });

        return CreatedAtAction(nameof(GetClients), new { veterinarianId }, result.Value);
    }

    [HttpDelete("{veterinarianId:int}/clients/{rancherId:int}")]
    public async Task<IActionResult> RemoveClient(int veterinarianId, int rancherId, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(
            new DeleteVeterinarianClientCommand(veterinarianId, rancherId),
            cancellationToken);

        if (result.IsFailure) return NotFound();

        return NoContent();
    }

    private static VeterinarianClientResource ToClientResource(
        VeterinarianClient relationship,
        User? rancher,
        IEnumerable<Herd> herds,
        IEnumerable<Animal> animals)
    {
        var rancherHerds = herds.Where(herd => herd.OwnerId == relationship.RancherId).ToList();
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
