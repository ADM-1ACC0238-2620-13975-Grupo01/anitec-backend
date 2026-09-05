namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

public record StripeCheckoutSessionResource(string SessionId, string CheckoutUrl);
