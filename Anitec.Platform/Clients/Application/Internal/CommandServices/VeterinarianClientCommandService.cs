using Anitec.Platform.Clients.Application.CommandServices;
using Anitec.Platform.Clients.Domain.Model;
using Anitec.Platform.Clients.Domain.Model.Commands;
using Anitec.Platform.Clients.Domain.Model.Entities;
using Anitec.Platform.Clients.Domain.Repositories;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Clients.Application.Internal.CommandServices;

/// <summary>
/// Links and unlinks veterinarians and ranchers. A pair can only be linked once.
/// </summary>
public class VeterinarianClientCommandService(
    IVeterinarianClientRepository repository,
    IUnitOfWork unitOfWork) : IVeterinarianClientCommandService
{
    /// <summary>Creates the link unless the same veterinarian and rancher are already linked.</summary>
    public async Task<Result<VeterinarianClient>> Handle(CreateVeterinarianClientCommand command, CancellationToken cancellationToken)
    {
        var exists = await repository.ExistsByVeterinarianIdAndRancherIdAsync(
            command.VeterinarianId,
            command.RancherId,
            cancellationToken);

        if (exists)
            return Result<VeterinarianClient>.Failure(
                ClientsError.VeterinarianClientAlreadyExists,
                "Veterinarian client relationship already exists.");

        var entity = new VeterinarianClient(command);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<VeterinarianClient>.Success(entity);
    }

    /// <summary>Removes the link found by veterinarian and rancher ids; fails when there is none.</summary>
    public async Task<Result> Handle(DeleteVeterinarianClientCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByVeterinarianIdAndRancherIdAsync(
            command.VeterinarianId,
            command.RancherId,
            cancellationToken);

        if (entity is null)
            return Result.Failure(ClientsError.VeterinarianClientNotFound, "Veterinarian client relationship not found.");

        repository.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}
