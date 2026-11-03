using Anitec.Platform.Activities.Application.CommandServices;
using Anitec.Platform.Activities.Domain.Model.Commands;
using Anitec.Platform.Activities.Domain.Model;
using Anitec.Platform.Activities.Domain.Model.Entities;
using Anitec.Platform.Activities.Domain.Repositories;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Activities.Application.Internal.CommandServices;

/// <summary>
/// Creates, updates and deletes farm activities and saves each change in one unit of work.
/// </summary>
public class FarmActivityCommandService(IFarmActivityRepository repository, IUnitOfWork unitOfWork)
    : IFarmActivityCommandService
{
    /// <summary>Stores a new activity built from the command.</summary>
    public async Task<Result<FarmActivity>> Handle(CreateFarmActivityCommand command, CancellationToken cancellationToken)
    {
        var entity = new FarmActivity(command);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<FarmActivity>.Success(entity);
    }

    /// <summary>Replaces the editable fields of an activity; fails when the id does not exist.</summary>
    public async Task<Result<FarmActivity>> Handle(UpdateFarmActivityCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result<FarmActivity>.Failure(ActivitiesError.FarmActivityNotFound, "FarmActivity not found.");
        entity.OwnerId = command.OwnerId;
        entity.VeterinarianId = command.VeterinarianId;
        entity.Title = command.Title;
        entity.Type = command.Type;
        entity.Date = command.Date;
        entity.Priority = command.Priority;
        entity.Status = command.Status;

        repository.Update(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<FarmActivity>.Success(entity);
    }

    /// <summary>Removes an activity; fails when the id does not exist.</summary>
    public async Task<Result> Handle(DeleteFarmActivityCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result.Failure(ActivitiesError.FarmActivityNotFound, "FarmActivity not found.");
        repository.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}
