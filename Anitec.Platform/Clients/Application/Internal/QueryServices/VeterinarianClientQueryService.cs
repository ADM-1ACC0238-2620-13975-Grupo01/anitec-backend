using Anitec.Platform.Clients.Application.QueryServices;
using Anitec.Platform.Clients.Domain.Model;
using Anitec.Platform.Clients.Domain.Model.Entities;
using Anitec.Platform.Clients.Domain.Model.Queries;
using Anitec.Platform.Clients.Domain.Repositories;

namespace Anitec.Platform.Clients.Application.Internal.QueryServices;

public class VeterinarianClientQueryService(IVeterinarianClientRepository repository) : IVeterinarianClientQueryService
{
    public async Task<IEnumerable<VeterinarianClient>> Handle(
        GetVeterinarianClientsByVeterinarianIdQuery query,
        CancellationToken cancellationToken)
    {
        return await repository.FindByVeterinarianIdAsync(query.VeterinarianId, cancellationToken);
    }

    /// <summary>Lists the links addressed to a rancher, only those in <c>query.Status</c> when one is given.</summary>
    public async Task<IEnumerable<VeterinarianClient>> Handle(
        GetVeterinarianLinksByRancherIdQuery query,
        CancellationToken cancellationToken)
    {
        var links = await repository.FindByRancherIdAsync(query.RancherId, cancellationToken);
        return string.IsNullOrWhiteSpace(query.Status)
            ? links
            : links.Where(link => ClientLinkStatus.Is(link.Status, query.Status)).ToList();
    }
}
