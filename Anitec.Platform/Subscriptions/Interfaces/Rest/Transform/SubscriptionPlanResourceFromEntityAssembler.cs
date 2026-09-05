using Anitec.Platform.Subscriptions.Domain.Model.Entities;
using Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Transform;

public static class SubscriptionPlanResourceFromEntityAssembler
{
    public static SubscriptionPlanResource ToResourceFromEntity(SubscriptionPlan entity)
    {
        return new SubscriptionPlanResource(entity.Id, entity.Name, entity.Price, entity.StripePriceId, entity.MaxAnimals, entity.IsActive);
    }
}