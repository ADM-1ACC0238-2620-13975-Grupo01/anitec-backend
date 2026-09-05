using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;
using Anitec.Platform.Subscriptions.Application.CommandServices;
using Anitec.Platform.Subscriptions.Domain.Model.Commands;
using Anitec.Platform.Subscriptions.Domain.Model.Entities;
using Anitec.Platform.Subscriptions.Domain.Repositories;

namespace Anitec.Platform.Subscriptions.Application.Internal.CommandServices;

public class PaymentCommandService(IPaymentRepository repository, IUnitOfWork unitOfWork) : IPaymentCommandService
{
    public async Task<Result<Payment>> Handle(CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        var payment = new Payment(command);
        await repository.AddAsync(payment, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Payment>.Success(payment);
    }
}
