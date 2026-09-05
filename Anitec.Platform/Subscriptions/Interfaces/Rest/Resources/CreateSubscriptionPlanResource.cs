namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

public record CreateSubscriptionPlanResource(string Name, decimal Price, string StripePriceId, int MaxAnimals, bool IsActive);
