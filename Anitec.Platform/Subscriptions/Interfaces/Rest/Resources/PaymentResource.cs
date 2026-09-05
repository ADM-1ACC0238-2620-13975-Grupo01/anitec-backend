namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

public record PaymentResource(
    int Id,
    int UserId,
    int SubscriptionId,
    decimal Amount,
    string Currency,
    string Provider,
    string ProviderPaymentId,
    string Status,
    DateTime PaidAt);
