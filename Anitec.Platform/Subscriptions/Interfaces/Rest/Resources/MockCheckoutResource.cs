namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

public record MockCheckoutResource(int UserId, int PlanId, decimal Amount, string Currency = "PEN");
