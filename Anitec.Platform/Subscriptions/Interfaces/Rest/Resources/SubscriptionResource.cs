namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

public record SubscriptionResource(int Id, int UserId, int PlanId, string StripeCustomerId, string StripeSubscriptionId, string Status, DateOnly StartedAt, DateOnly? EndsAt);
