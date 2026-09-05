using Anitec.Platform.Subscriptions.Application.CommandServices;
using Anitec.Platform.Subscriptions.Domain.Model.Commands;
using Anitec.Platform.Subscriptions.Domain.Model;
using Anitec.Platform.Subscriptions.Domain.Model.Entities;
using Anitec.Platform.Subscriptions.Domain.Repositories;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Subscriptions.Application.Internal.CommandServices;

public class SubscriptionPlanCommandService(ISubscriptionPlanRepository repository, IUnitOfWork unitOfWork)
    : ISubscriptionPlanCommandService
{
    public async Task<Result<SubscriptionPlan>> Handle(CreateSubscriptionPlanCommand command, CancellationToken cancellationToken)
    {
        var entity = new SubscriptionPlan(command);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<SubscriptionPlan>.Success(entity);
    }

    public async Task<Result<SubscriptionPlan>> Handle(UpdateSubscriptionPlanCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result<SubscriptionPlan>.Failure(SubscriptionsError.SubscriptionPlanNotFound, "SubscriptionPlan not found.");
        entity.Name = command.Name;
        entity.Price = command.Price;
        entity.StripePriceId = command.StripePriceId;
        entity.MaxAnimals = command.MaxAnimals;
        entity.IsActive = command.IsActive;

        repository.Update(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<SubscriptionPlan>.Success(entity);
    }

    public async Task<Result> Handle(DeleteSubscriptionPlanCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result.Failure(SubscriptionsError.SubscriptionPlanNotFound, "SubscriptionPlan not found.");
        repository.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}
