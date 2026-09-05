using Anitec.Platform.Livestock.Application.CommandServices;
using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Domain.Model;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Repositories;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Livestock.Application.Internal.CommandServices;

public class HerdCommandService(IHerdRepository repository, IUnitOfWork unitOfWork)
    : IHerdCommandService
{
    public async Task<Result<Herd>> Handle(CreateHerdCommand command, CancellationToken cancellationToken)
    {
        var entity = new Herd(command);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Herd>.Success(entity);
    }

    public async Task<Result<Herd>> Handle(UpdateHerdCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result<Herd>.Failure(LivestockError.HerdNotFound, "Herd not found.");
        entity.Name = command.Name;
        entity.Location = command.Location;
        entity.Owner = command.Owner;
        entity.OwnerId = command.OwnerId;
        entity.VeterinarianId = command.VeterinarianId;
        entity.MainType = command.MainType;

        repository.Update(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Herd>.Success(entity);
    }

    public async Task<Result> Handle(DeleteHerdCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result.Failure(LivestockError.HerdNotFound, "Herd not found.");
        repository.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}
