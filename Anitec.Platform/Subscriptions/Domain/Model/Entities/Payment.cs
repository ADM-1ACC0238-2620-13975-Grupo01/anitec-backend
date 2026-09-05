using Anitec.Platform.Subscriptions.Domain.Model.Commands;

namespace Anitec.Platform.Subscriptions.Domain.Model.Entities;

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
    public string Provider { get; set; }
    public string ProviderPaymentId { get; set; }
    public string Status { get; set; }
    public DateTime PaidAt { get; set; }
}
