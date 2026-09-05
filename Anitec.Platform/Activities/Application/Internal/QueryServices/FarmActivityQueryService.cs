using Anitec.Platform.Activities.Application.QueryServices;
using Anitec.Platform.Activities.Domain.Model.Entities;
using Anitec.Platform.Activities.Domain.Model.Queries;
using Anitec.Platform.Activities.Domain.Repositories;

namespace Anitec.Platform.Activities.Application.Internal.QueryServices;

public class FarmActivityQueryService(IFarmActivityRepository repository) : IFarmActivityQueryService
{
    public async Task<FarmActivity?> Handle(GetFarmActivityByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<IEnumerable<FarmActivity>> Handle(GetAllFarmActivitiesQuery query, CancellationToken cancellationToken)
    {
        return await repository.ListAsync(cancellationToken);
    }
}


