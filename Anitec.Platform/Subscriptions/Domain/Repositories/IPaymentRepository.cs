using Anitec.Platform.Shared.Domain.Repositories;
using Anitec.Platform.Subscriptions.Domain.Model.Entities;

namespace Anitec.Platform.Subscriptions.Domain.Repositories;

public interface IPaymentRepository : IBaseRepository<Payment>
{
    Task<IEnumerable<Payment>> FindByUserIdAsync(int userId, CancellationToken cancellationToken);
    Task<Payment?> FindByProviderPaymentIdAsync(string providerPaymentId, CancellationToken cancellationToken);
}
