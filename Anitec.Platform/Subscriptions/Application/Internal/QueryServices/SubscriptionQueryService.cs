using Anitec.Platform.Subscriptions.Application.QueryServices;
using Anitec.Platform.Subscriptions.Domain.Model.Entities;
using Anitec.Platform.Subscriptions.Domain.Model.Queries;
using Anitec.Platform.Subscriptions.Domain.Repositories;

namespace Anitec.Platform.Subscriptions.Application.Internal.QueryServices;

/// <summary>
/// Read-only queries over subscriptions.
/// </summary>
public class SubscriptionQueryService(ISubscriptionRepository repository) : ISubscriptionQueryService
{
    /// <summary>Subscription with the given id, or null.</summary>
    public async Task<Subscription?> Handle(GetSubscriptionByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    /// <summary>Every stored subscription.</summary>
    public async Task<IEnumerable<Subscription>> Handle(GetAllSubscriptionsQuery query, CancellationToken cancellationToken)
    {
        return await repository.ListAsync(cancellationToken);
    }
}




