namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

public record CreateSubscriptionResource(int UserId, int PlanId, string StripeCustomerId, string StripeSubscriptionId, string Status, DateOnly StartedAt, DateOnly? EndsAt);
