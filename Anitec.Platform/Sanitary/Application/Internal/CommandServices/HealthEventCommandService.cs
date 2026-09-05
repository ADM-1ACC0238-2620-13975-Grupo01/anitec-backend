using Anitec.Platform.Sanitary.Application.CommandServices;
using Anitec.Platform.Sanitary.Domain.Model.Commands;
using Anitec.Platform.Sanitary.Domain.Model;
using Anitec.Platform.Sanitary.Domain.Model.Entities;
using Anitec.Platform.Sanitary.Domain.Repositories;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Sanitary.Application.Internal.CommandServices;

public class HealthEventCommandService(IHealthEventRepository repository, IUnitOfWork unitOfWork)
    : IHealthEventCommandService
{
    public async Task<Result<HealthEvent>> Handle(CreateHealthEventCommand command, CancellationToken cancellationToken)
    {
        var entity = new HealthEvent(command);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<HealthEvent>.Success(entity);
    }

    public async Task<Result<HealthEvent>> Handle(UpdateHealthEventCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result<HealthEvent>.Failure(SanitaryError.HealthEventNotFound, "HealthEvent not found.");
        entity.AnimalId = command.AnimalId;
        entity.Type = command.Type;
        entity.Date = command.Date;
        entity.Description = command.Description;
        entity.Veterinarian = command.Veterinarian;
        entity.Diagnosis = command.Diagnosis;
        entity.Treatment = command.Treatment;
        entity.Prescription = command.Prescription;
        entity.FollowUp = command.FollowUp;
        entity.NextDueDate = command.NextDueDate;

        repository.Update(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<HealthEvent>.Success(entity);
    }

    public async Task<Result> Handle(DeleteHealthEventCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result.Failure(SanitaryError.HealthEventNotFound, "HealthEvent not found.");
        repository.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}
