using Anitec.Platform.Livestock.Application.QueryServices;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Model.Queries;
using Anitec.Platform.Livestock.Domain.Repositories;

namespace Anitec.Platform.Livestock.Application.Internal.QueryServices;

/// <summary>
/// Read-only queries over herds.
/// </summary>
public class HerdQueryService(IHerdRepository repository) : IHerdQueryService
{
    /// <summary>Herd with the given id, or null.</summary>
    public async Task<Herd?> Handle(GetHerdByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    /// <summary>Every herd; filtering by user is done by the caller.</summary>
    public async Task<IEnumerable<Herd>> Handle(GetAllHerdsQuery query, CancellationToken cancellationToken)
    {
        return await repository.ListAsync(cancellationToken);
    }
}




