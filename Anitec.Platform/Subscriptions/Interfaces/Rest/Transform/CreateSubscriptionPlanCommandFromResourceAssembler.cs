using Anitec.Platform.Subscriptions.Domain.Model.Commands;
using Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Transform;

public static class CreateSubscriptionPlanCommandFromResourceAssembler
{
    public static CreateSubscriptionPlanCommand ToCommandFromResource(CreateSubscriptionPlanResource resource)
    {
        return new CreateSubscriptionPlanCommand(resource.Name, resource.Price, resource.StripePriceId, resource.MaxAnimals, resource.IsActive);
    }
}