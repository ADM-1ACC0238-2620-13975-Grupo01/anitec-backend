using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Anitec.Platform.Subscriptions.Domain.Model.Entities;
using Anitec.Platform.Subscriptions.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Anitec.Platform.Subscriptions.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class PaymentRepository(AppDbContext context) : BaseRepository<Payment>(context), IPaymentRepository
{
    public async Task<IEnumerable<Payment>> FindByUserIdAsync(int userId, CancellationToken cancellationToken)
    {
        return await Context.Set<Payment>()
            .Where(payment => payment.UserId == userId)
            .OrderByDescending(payment => payment.PaidAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Payment?> FindByProviderPaymentIdAsync(string providerPaymentId, CancellationToken cancellationToken)
    {
        return await Context.Set<Payment>()
            .FirstOrDefaultAsync(payment => payment.ProviderPaymentId == providerPaymentId, cancellationToken);
    }
}
