using Anitec.Platform.Subscriptions.Domain.Model.Commands;
using Anitec.Platform.Subscriptions.Domain.Model;
using Anitec.Platform.Subscriptions.Domain.Model.Entities;
using Anitec.Platform.Shared.Application.Model;

namespace Anitec.Platform.Subscriptions.Application.CommandServices;

public interface ISubscriptionPlanCommandService
{
    Task<Result<SubscriptionPlan>> Handle(CreateSubscriptionPlanCommand command, CancellationToken cancellationToken);
    Task<Result<SubscriptionPlan>> Handle(UpdateSubscriptionPlanCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteSubscriptionPlanCommand command, CancellationToken cancellationToken);
}
