using Anitec.Platform.Subscriptions.Domain.Model.Entities;
using Anitec.Platform.Subscriptions.Interfaces.Rest.Resources;

namespace Anitec.Platform.Subscriptions.Interfaces.Rest.Transform;

public static class PaymentResourceFromEntityAssembler
{
    public static PaymentResource ToResourceFromEntity(Payment payment)
    {
        return new PaymentResource(
            payment.Id,
            payment.UserId,
            payment.SubscriptionId,
            payment.Amount,
            payment.Currency,
            payment.Provider,
            payment.ProviderPaymentId,
            payment.Status,
            payment.PaidAt);
    }
}
