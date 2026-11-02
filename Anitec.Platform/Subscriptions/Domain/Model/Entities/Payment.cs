using Anitec.Platform.Subscriptions.Domain.Model.Commands;

namespace Anitec.Platform.Subscriptions.Domain.Model.Entities;

/// <summary>
/// A payment made for a subscription. Defaults to a paid mock Stripe payment in PEN.
/// </summary>
public class Payment
{
    public Payment()
    {
        Provider = "MockStripe";
        ProviderPaymentId = string.Empty;
        Status = "Paid";
        Currency = "PEN";
    }

    public Payment(CreatePaymentCommand command)
    {
        UserId = command.UserId;
        SubscriptionId = command.SubscriptionId;
        Amount = command.Amount;
        Currency = command.Currency;
        Provider = command.Provider;
        ProviderPaymentId = command.ProviderPaymentId;
        Status = command.Status;
        PaidAt = command.PaidAt;
    }

    public int Id { get; set; }
    public int UserId { get; set; }
    public int SubscriptionId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    /// <summary>Payment provider that processed the payment.</summary>
    public string Provider { get; set; }
    /// <summary>Id of the payment in the provider.</summary>
    public string ProviderPaymentId { get; set; }
    public string Status { get; set; }
    /// <summary>Moment the payment was completed.</summary>
    public DateTime PaidAt { get; set; }
}
