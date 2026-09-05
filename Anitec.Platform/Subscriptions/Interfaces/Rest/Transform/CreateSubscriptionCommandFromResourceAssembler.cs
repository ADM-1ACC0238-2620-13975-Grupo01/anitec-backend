using Anitec.Platform.Subscriptions.Domain.Model.Commands;
using Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Transform;

public static class CreateSubscriptionCommandFromResourceAssembler
{
    public static CreateSubscriptionCommand ToCommandFromResource(CreateSubscriptionResource resource)
    {
        return new CreateSubscriptionCommand(resource.UserId, resource.PlanId, resource.StripeCustomerId, resource.StripeSubscriptionId, resource.Status, resource.StartedAt, resource.EndsAt);
    }
}