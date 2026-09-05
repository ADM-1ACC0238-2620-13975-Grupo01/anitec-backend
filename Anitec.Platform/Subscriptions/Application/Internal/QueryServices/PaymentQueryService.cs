using Anitec.Platform.Subscriptions.Application.QueryServices;
using Anitec.Platform.Subscriptions.Domain.Model.Entities;
using Anitec.Platform.Subscriptions.Domain.Repositories;

namespace Anitec.Platform.Subscriptions.Application.Internal.QueryServices;

public class PaymentQueryService(IPaymentRepository repository) : IPaymentQueryService
{
    public async Task<IEnumerable<Payment>> Handle(int userId, CancellationToken cancellationToken)
    {
        return await repository.FindByUserIdAsync(userId, cancellationToken);
    }

    public async Task<Payment?> Handle(string providerPaymentId, CancellationToken cancellationToken)
    {
        return await repository.FindByProviderPaymentIdAsync(providerPaymentId, cancellationToken);
    }
}
