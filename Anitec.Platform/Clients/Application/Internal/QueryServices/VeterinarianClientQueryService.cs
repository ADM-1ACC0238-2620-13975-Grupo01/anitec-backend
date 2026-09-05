using Anitec.Platform.Clients.Application.QueryServices;
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
}
