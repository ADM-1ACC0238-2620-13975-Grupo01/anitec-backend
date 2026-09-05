using Anitec.Platform.Subscriptions.Domain.Model.Commands;

namespace Anitec.Platform.Subscriptions.Domain.Model.Entities;

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
    public string StripeCustomerId { get; set; }
    public string StripeSubscriptionId { get; set; }
    public string Status { get; set; }
    public DateOnly StartedAt { get; set; }
    public DateOnly? EndsAt { get; set; }
}
