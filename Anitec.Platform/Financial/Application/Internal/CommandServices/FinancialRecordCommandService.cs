using Anitec.Platform.Financial.Application.CommandServices;
using Anitec.Platform.Financial.Domain.Model.Commands;
using Anitec.Platform.Financial.Domain.Model;
using Anitec.Platform.Financial.Domain.Model.Entities;
using Anitec.Platform.Financial.Domain.Repositories;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Financial.Application.Internal.CommandServices;

/// <summary>
/// Writes the income and expense records of a rancher.
/// </summary>
public class FinancialRecordCommandService(IFinancialRecordRepository repository, IUnitOfWork unitOfWork)
    : IFinancialRecordCommandService
{
    /// <summary>Saves a new income or expense record.</summary>
    public async Task<Result<FinancialRecord>> Handle(CreateFinancialRecordCommand command, CancellationToken cancellationToken)
    {
        var entity = new FinancialRecord(command);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<FinancialRecord>.Success(entity);
    }

    /// <summary>Updates owner, type, category, amount, date and description of a record.</summary>
    public async Task<Result<FinancialRecord>> Handle(UpdateFinancialRecordCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result<FinancialRecord>.Failure(FinancialError.FinancialRecordNotFound, "FinancialRecord not found.");
        entity.OwnerId = command.OwnerId;
        entity.Type = command.Type;
        entity.Category = command.Category;
        entity.Amount = command.Amount;
        entity.Date = command.Date;
        entity.Description = command.Description;

        repository.Update(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<FinancialRecord>.Success(entity);
    }

    /// <summary>Deletes a record; fails when the id does not exist.</summary>
    public async Task<Result> Handle(DeleteFinancialRecordCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result.Failure(FinancialError.FinancialRecordNotFound, "FinancialRecord not found.");
        repository.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}