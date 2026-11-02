using Anitec.Platform.Subscriptions.Domain.Model.Commands;

namespace Anitec.Platform.Subscriptions.Domain.Model.Entities;

/// <summary>
/// A user's subscription to a plan, with the ids of the matching Stripe customer and subscription.
/// </summary>
public class Subscription
{
    public Subscription()
    {
        StripeCustomerId = string.Empty;
        StripeSubscriptionId = string.Empty;
        Status = string.Empty;
    }

    public Subscription(CreateSubscriptionCommand command)
    {
        UserId = command.UserId;
        PlanId = command.PlanId;
        StripeCustomerId = command.StripeCustomerId;
        StripeSubscriptionId = command.StripeSubscriptionId;
        Status = command.Status;
        StartedAt = command.StartedAt;
        EndsAt = command.EndsAt;

    }

    public int Id { get; set; }
    public int UserId { get; set; }
    public int PlanId { get; set; }
    /// <summary>Id of the customer in Stripe.</summary>
    public string StripeCustomerId { get; set; }
    /// <summary>Id of the subscription in Stripe.</summary>
    public string StripeSubscriptionId { get; set; }
    public string Status { get; set; }
    public DateOnly StartedAt { get; set; }
    /// <summary>Day the subscription ends, or null while it renews.</summary>
    public DateOnly? EndsAt { get; set; }
}
