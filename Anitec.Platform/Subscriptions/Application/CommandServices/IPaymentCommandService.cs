using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Subscriptions.Domain.Model.Commands;
using Anitec.Platform.Subscriptions.Domain.Model.Entities;

namespace Anitec.Platform.Subscriptions.Application.CommandServices;

public interface IPaymentCommandService
{
    Task<Result<Payment>> Handle(CreatePaymentCommand command, CancellationToken cancellationToken);
}
