namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

/// <summary>
///     Starts a Stripe checkout for a plan. <paramref name="Platform" /> is "android" when the app opens the
///     checkout in a browser tab; Stripe then returns to a page of this API that opens the app.
/// </summary>
public record StripeCheckoutResource(int PlanId, string? Platform = null);
