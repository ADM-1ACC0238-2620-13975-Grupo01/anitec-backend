using Anitec.Platform.Livestock.Application.QueryServices;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Model.Queries;
using Anitec.Platform.Livestock.Domain.Repositories;

namespace Anitec.Platform.Livestock.Application.Internal.QueryServices;

public class CorralQueryService(ICorralRepository repository) : ICorralQueryService
{
    public async Task<Corral?> Handle(GetCorralByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<IEnumerable<Corral>> Handle(GetAllCorralsQuery query, CancellationToken cancellationToken)
    {
        return await repository.ListAsync(cancellationToken);
    }
}
