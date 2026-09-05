using Anitec.Platform.Subscriptions.Application.QueryServices;
using Anitec.Platform.Subscriptions.Domain.Model.Entities;
using Anitec.Platform.Subscriptions.Domain.Model.Queries;
using Anitec.Platform.Subscriptions.Domain.Repositories;

namespace Anitec.Platform.Subscriptions.Application.Internal.QueryServices;

public class SubscriptionPlanQueryService(ISubscriptionPlanRepository repository) : ISubscriptionPlanQueryService
{
    public async Task<SubscriptionPlan?> Handle(GetSubscriptionPlanByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<IEnumerable<SubscriptionPlan>> Handle(GetAllSubscriptionPlansQuery query, CancellationToken cancellationToken)
    {
        return await repository.ListAsync(cancellationToken);
    }
}




