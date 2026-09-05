namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

public record SubscriptionPlanResource(int Id, string Name, decimal Price, string StripePriceId, int MaxAnimals, bool IsActive);
