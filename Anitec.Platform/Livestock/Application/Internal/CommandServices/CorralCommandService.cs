using Anitec.Platform.Livestock.Application.CommandServices;
using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Domain.Model;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Repositories;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Livestock.Application.Internal.CommandServices;

public class CorralCommandService(ICorralRepository repository, IUnitOfWork unitOfWork)
    : ICorralCommandService
{
    public async Task<Result<Corral>> Handle(CreateCorralCommand command, CancellationToken cancellationToken)
    {
        var entity = new Corral(command);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Corral>.Success(entity);
    }

    public async Task<Result<Corral>> Handle(UpdateCorralCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result<Corral>.Failure(LivestockError.CorralNotFound, "Corral not found.");
        entity.Name = command.Name;
        entity.HerdId = command.HerdId;

        repository.Update(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Corral>.Success(entity);
    }

    public async Task<Result> Handle(DeleteCorralCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result.Failure(LivestockError.CorralNotFound, "Corral not found.");
        repository.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}
