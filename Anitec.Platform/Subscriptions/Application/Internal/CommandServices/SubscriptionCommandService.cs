using Anitec.Platform.Subscriptions.Application.CommandServices;
using Anitec.Platform.Subscriptions.Domain.Model.Commands;
using Anitec.Platform.Subscriptions.Domain.Model;
using Anitec.Platform.Subscriptions.Domain.Model.Entities;
using Anitec.Platform.Subscriptions.Domain.Repositories;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Subscriptions.Application.Internal.CommandServices;

public class SubscriptionCommandService(ISubscriptionRepository repository, IUnitOfWork unitOfWork)
    : ISubscriptionCommandService
{
    public async Task<Result<Subscription>> Handle(CreateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var entity = new Subscription(command);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Subscription>.Success(entity);
    }

    public async Task<Result<Subscription>> Handle(UpdateSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result<Subscription>.Failure(SubscriptionsError.SubscriptionNotFound, "Subscription not found.");
        entity.UserId = command.UserId;
        entity.PlanId = command.PlanId;
        entity.StripeCustomerId = command.StripeCustomerId;
        entity.StripeSubscriptionId = command.StripeSubscriptionId;
        entity.Status = command.Status;
        entity.StartedAt = command.StartedAt;
        entity.EndsAt = command.EndsAt;

        repository.Update(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Subscription>.Success(entity);
    }

    public async Task<Result> Handle(DeleteSubscriptionCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result.Failure(SubscriptionsError.SubscriptionNotFound, "Subscription not found.");
        repository.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}
