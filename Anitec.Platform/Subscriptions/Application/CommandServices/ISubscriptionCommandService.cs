using Anitec.Platform.Subscriptions.Domain.Model.Commands;
using Anitec.Platform.Subscriptions.Domain.Model;
using Anitec.Platform.Subscriptions.Domain.Model.Entities;
using Anitec.Platform.Shared.Application.Model;

namespace Anitec.Platform.Subscriptions.Application.CommandServices;

public interface ISubscriptionCommandService
{
    Task<Result<Subscription>> Handle(CreateSubscriptionCommand command, CancellationToken cancellationToken);
    Task<Result<Subscription>> Handle(UpdateSubscriptionCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteSubscriptionCommand command, CancellationToken cancellationToken);
}
