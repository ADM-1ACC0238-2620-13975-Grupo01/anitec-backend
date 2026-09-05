using Anitec.Platform.Subscriptions.Domain.Model.Commands;
using Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Transform;

public static class UpdateSubscriptionPlanCommandFromResourceAssembler
{
    public static UpdateSubscriptionPlanCommand ToCommandFromResource(int id, CreateSubscriptionPlanResource resource)
    {
        return new UpdateSubscriptionPlanCommand(id, resource.Name, resource.Price, resource.StripePriceId, resource.MaxAnimals, resource.IsActive);
    }
}