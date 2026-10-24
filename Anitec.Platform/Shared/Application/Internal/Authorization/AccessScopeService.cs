using Anitec.Platform.Clients.Domain.Repositories;
using Anitec.Platform.Iam.Domain.Model.Aggregates;
using Anitec.Platform.Livestock.Domain.Repositories;

namespace Anitec.Platform.Shared.Application.Internal.Authorization;

/// <summary>Builds the <see cref="AccessScope" /> of a user from the herds, animals and client links.</summary>
public interface IAccessScopeService
{
    /// <summary>Resolves what <paramref name="user" /> is allowed to see.</summary>
    Task<AccessScope> GetAsync(User user, CancellationToken cancellationToken = default);
}

/// <inheritdoc />
public class AccessScopeService(
    IHerdRepository herdRepository,
    IAnimalRepository animalRepository,
    IVeterinarianClientRepository clientRepository) : IAccessScopeService
{
    private const string VeterinarianRole = "Veterinarian";
    private const string AcceptedStatus = "Accepted";

    /// <inheritdoc />
    public async Task<AccessScope> GetAsync(User user, CancellationToken cancellationToken = default)
    {
        var isVeterinarian = VeterinarianRole.Equals(user.Role, StringComparison.OrdinalIgnoreCase);

        var clientRancherIds = new HashSet<int>();
        if (isVeterinarian)
            clientRancherIds = (await clientRepository.FindByVeterinarianIdAsync(user.Id, cancellationToken))
                .Where(client => AcceptedStatus.Equals(client.Status, StringComparison.OrdinalIgnoreCase))
                .Select(client => client.RancherId)
                .ToHashSet();

        var herdIds = (await herdRepository.ListAsync(cancellationToken))
            .Where(herd => isVeterinarian
                ? clientRancherIds.Contains(herd.OwnerId) || herd.VeterinarianId == user.Id
                : herd.OwnerId == user.Id)
            .Select(herd => herd.Id)
            .ToHashSet();

        var animalIds = (await animalRepository.ListAsync(cancellationToken))
            .Where(animal => herdIds.Contains(animal.HerdId))
            .Select(animal => animal.Id)
            .ToHashSet();

        return new AccessScope(user.Id, isVeterinarian, clientRancherIds, herdIds, animalIds);
    }
}
