using Anitec.Platform.Livestock.Application.CommandServices;
using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Domain.Model;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Repositories;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Livestock.Application.Internal.CommandServices;

public class AnimalCommandService(IAnimalRepository repository, IUnitOfWork unitOfWork)
    : IAnimalCommandService
{
    public async Task<Result<Animal>> Handle(CreateAnimalCommand command, CancellationToken cancellationToken)
    {
        var entity = new Animal(command);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Animal>.Success(entity);
    }

    public async Task<Result<Animal>> Handle(UpdateAnimalCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result<Animal>.Failure(LivestockError.AnimalNotFound, "Animal not found.");
        entity.Tag = command.Tag;
        entity.Name = command.Name;
        entity.Species = command.Species;
        entity.Breed = command.Breed;
        entity.Gender = command.Gender;
        entity.BirthDate = command.BirthDate;
        entity.Weight = command.Weight;
        entity.Status = command.Status;
        entity.HerdId = command.HerdId;

        repository.Update(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Animal>.Success(entity);
    }

    public async Task<Result> Handle(DeleteAnimalCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result.Failure(LivestockError.AnimalNotFound, "Animal not found.");
        repository.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}
