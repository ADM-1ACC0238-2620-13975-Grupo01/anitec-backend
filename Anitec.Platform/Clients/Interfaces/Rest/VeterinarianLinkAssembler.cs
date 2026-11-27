using Anitec.Platform.Clients.Domain.Model.Entities;
using Anitec.Platform.Clients.Interfaces.Rest.Resources;
using Anitec.Platform.Iam.Domain.Model.Aggregates;

namespace Anitec.Platform.Clients.Interfaces.Rest;

/// <summary>Builds the link resource, adding the identity of the veterinarian from the user list.</summary>
public static class VeterinarianLinkAssembler
{
    public static VeterinarianLinkResource ToResource(VeterinarianClient link, IReadOnlyCollection<User> users)
    {
        var veterinarian = users.FirstOrDefault(user => user.Id == link.VeterinarianId);
        return new VeterinarianLinkResource(
            link.Id,
            link.VeterinarianId,
            veterinarian?.FullName ?? "Unknown veterinarian",
            veterinarian?.Username ?? string.Empty,
            link.RancherId,
            link.Status,
            link.RequestedAt,
            link.AcceptedAt,
            link.ClosedAt);
    }
}
