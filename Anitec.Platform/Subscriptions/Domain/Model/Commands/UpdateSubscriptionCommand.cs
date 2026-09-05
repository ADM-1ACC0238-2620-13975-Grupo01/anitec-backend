namespace Anitec.Platform.Subscriptions.Domain.Model.Commands;

public record UpdateSubscriptionCommand(int Id, int UserId, int PlanId, string StripeCustomerId, string StripeSubscriptionId, string Status, DateOnly StartedAt, DateOnly? EndsAt);
