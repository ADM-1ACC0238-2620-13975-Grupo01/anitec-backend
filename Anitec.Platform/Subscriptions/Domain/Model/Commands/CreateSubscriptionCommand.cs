namespace Anitec.Platform.Subscriptions.Domain.Model.Commands;

public record CreateSubscriptionCommand(int UserId, int PlanId, string StripeCustomerId, string StripeSubscriptionId, string Status, DateOnly StartedAt, DateOnly? EndsAt);
