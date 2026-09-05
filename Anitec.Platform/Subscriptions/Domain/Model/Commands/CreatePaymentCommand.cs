namespace Anitec.Platform.Subscriptions.Domain.Model.Commands;

public record CreatePaymentCommand(
    int UserId,
    int SubscriptionId,
    decimal Amount,
    string Currency,
    string Provider,
    string ProviderPaymentId,
    string Status,
    DateTime PaidAt);
