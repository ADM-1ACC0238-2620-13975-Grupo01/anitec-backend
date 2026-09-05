using Anitec.Platform.Subscriptions.Domain.Model.Commands;
using Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Transform;

public static class UpdateSubscriptionCommandFromResourceAssembler
{
    public static UpdateSubscriptionCommand ToCommandFromResource(int id, CreateSubscriptionResource resource)
    {
        return new UpdateSubscriptionCommand(id, resource.UserId, resource.PlanId, resource.StripeCustomerId, resource.StripeSubscriptionId, resource.Status, resource.StartedAt, resource.EndsAt);
    }
}