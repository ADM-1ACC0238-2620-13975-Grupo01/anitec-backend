using Anitec.Platform.Activities.Domain.Model.Entities;
using Anitec.Platform.Activities.Domain.Model.Queries;

namespace Anitec.Platform.Activities.Application.QueryServices;

public interface IFarmActivityQueryService
{
    Task<FarmActivity?> Handle(GetFarmActivityByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<FarmActivity>> Handle(GetAllFarmActivitiesQuery query, CancellationToken cancellationToken);
}




