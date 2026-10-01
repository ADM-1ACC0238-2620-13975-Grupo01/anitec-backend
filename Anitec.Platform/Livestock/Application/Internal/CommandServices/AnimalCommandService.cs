using Anitec.Platform.Livestock.Application.CommandServices;
using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Domain.Model;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Repositories;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Livestock.Application.Internal.CommandServices;

public class AnimalCommandService(IAnimalRepository repository, ICorralRepository corralRepository, IUnitOfWork unitOfWork)
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
        entity.CorralId = command.CorralId;
        entity.Source = command.Source;
        entity.AgeRange = command.AgeRange;
        entity.ImageUrl = command.ImageUrl;

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

    public async Task<Result<List<Animal>>> Handle(CreateAnimalBatchCommand command, CancellationToken cancellationToken)
    {
        if (command.Quantity is < 1 or > 500)
            return Result<List<Animal>>.Failure(LivestockError.AnimalNotFound, "Quantity must be between 1 and 500.");

        var corral = await corralRepository.FindByIdAsync(command.CorralId, cancellationToken);
        if (corral is null) return Result<List<Animal>>.Failure(LivestockError.CorralNotFound, "Corral not found.");

        var allAnimals = await repository.ListAsync(cancellationToken);
        var existingInCorral = allAnimals.Count(a => a.CorralId == command.CorralId);

        var slug = new string(corral.Name.Where(char.IsLetterOrDigit).ToArray());
        if (string.IsNullOrWhiteSpace(slug)) slug = "CORRAL";

        var created = new List<Animal>();
        for (var i = 1; i <= command.Quantity; i++)
        {
            var sequence = existingInCorral + i;
            var tag = $"{slug}-{sequence:000}";
            var createCommand = new CreateAnimalCommand(tag, tag, command.Species, command.Breed, command.Gender,
                command.BirthDate, command.Weight, command.Status, command.HerdId, command.CorralId,
                command.Source, command.AgeRange, command.ImageUrl);
            var entity = new Animal(createCommand);
            await repository.AddAsync(entity, cancellationToken);
            created.Add(entity);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<List<Animal>>.Success(created);
    }

    public async Task<Result<List<Animal>>> Handle(UpdateAnimalsStatusCommand command, CancellationToken cancellationToken)
    {
        if (command.AnimalIds.Count == 0)
            return Result<List<Animal>>.Failure(LivestockError.AnimalNotFound, "No animals were selected.");

        var updated = new List<Animal>();
        foreach (var id in command.AnimalIds)
        {
            var entity = await repository.FindByIdAsync(id, cancellationToken);
            if (entity is null) continue;
            entity.Status = command.Status;
            repository.Update(entity);
            updated.Add(entity);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<List<Animal>>.Success(updated);
    }

    public async Task<Result> Handle(DeleteAnimalsCommand command, CancellationToken cancellationToken)
    {
        if (command.AnimalIds.Count == 0)
            return Result.Failure(LivestockError.AnimalNotFound, "No animals were selected.");

        foreach (var id in command.AnimalIds)
        {
            var entity = await repository.FindByIdAsync(id, cancellationToken);
            if (entity is null) continue;
            repository.Remove(entity);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}
