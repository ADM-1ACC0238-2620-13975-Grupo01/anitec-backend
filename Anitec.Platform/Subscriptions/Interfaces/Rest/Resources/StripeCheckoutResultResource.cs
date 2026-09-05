namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

public record StripeCheckoutResultResource(SubscriptionResource Subscription, PaymentResource Payment);
