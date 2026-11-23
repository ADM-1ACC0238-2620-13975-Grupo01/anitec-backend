using Anitec.Platform.Clients.Application.CommandServices;
using Anitec.Platform.Clients.Domain.Model;
using Anitec.Platform.Clients.Domain.Model.Commands;
using Anitec.Platform.Clients.Domain.Model.Entities;
using Anitec.Platform.Clients.Domain.Repositories;
using Anitec.Platform.Livestock.Domain.Repositories;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Clients.Application.Internal.CommandServices;

/// <summary>
/// Runs the consent flow between veterinarians and ranchers: the veterinarian asks, the rancher accepts or
/// rejects, and the rancher can revoke an accepted link. A pair has a single link that moves between states.
/// </summary>
public class VeterinarianClientCommandService(
    IVeterinarianClientRepository repository,
    IHerdRepository herdRepository,
    IUnitOfWork unitOfWork) : IVeterinarianClientCommandService
{
    /// <summary>
    /// Opens a request. A pending or accepted link for the same pair is never duplicated; a link that was
    /// rejected or revoked earlier goes back to pending.
    /// </summary>
    public async Task<Result<VeterinarianClient>> Handle(CreateVeterinarianClientCommand command, CancellationToken cancellationToken)
    {
        var existing = await repository.FindByVeterinarianIdAndRancherIdAsync(
            command.VeterinarianId,
            command.RancherId,
            cancellationToken);

        if (existing is not null)
        {
            if (existing.IsPending)
                return Result<VeterinarianClient>.Failure(
                    ClientsError.RequestAlreadyPending,
                    "A request to this rancher is already waiting for an answer.");

            if (existing.IsActive)
                return Result<VeterinarianClient>.Failure(
                    ClientsError.VeterinarianClientAlreadyExists,
                    "Veterinarian client relationship already exists.");

            existing.Reopen();
            repository.Update(existing);
            await unitOfWork.CompleteAsync(cancellationToken);
            return Result<VeterinarianClient>.Success(existing);
        }

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

    /// <summary>Accepts a pending request addressed to the rancher; the veterinarian gets access.</summary>
    public async Task<Result<VeterinarianClient>> Handle(AcceptVeterinarianRequestCommand command, CancellationToken cancellationToken)
    {
        var link = await FindOwnedAsync(command.LinkId, command.RancherId, cancellationToken);
        if (link is null) return NotFound();

        if (!link.Accept())
            return Result<VeterinarianClient>.Failure(ClientsError.RequestNotPending, "The request was already answered.");

        repository.Update(link);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<VeterinarianClient>.Success(link);
    }

    /// <summary>Rejects a pending request addressed to the rancher; no access is granted.</summary>
    public async Task<Result<VeterinarianClient>> Handle(RejectVeterinarianRequestCommand command, CancellationToken cancellationToken)
    {
        var link = await FindOwnedAsync(command.LinkId, command.RancherId, cancellationToken);
        if (link is null) return NotFound();

        if (!link.Reject())
            return Result<VeterinarianClient>.Failure(ClientsError.RequestNotPending, "The request was already answered.");

        repository.Update(link);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<VeterinarianClient>.Success(link);
    }

    /// <summary>
    /// Withdraws the access of an accepted veterinarian. The veterinarian is also removed from the herds of the
    /// rancher that name them, because a herd gives its veterinarian access too. Records they wrote keep their author.
    /// </summary>
    public async Task<Result<VeterinarianClient>> Handle(RevokeVeterinarianAccessCommand command, CancellationToken cancellationToken)
    {
        var link = await FindOwnedAsync(command.LinkId, command.RancherId, cancellationToken);
        if (link is null) return NotFound();

        if (!link.Revoke())
            return Result<VeterinarianClient>.Failure(ClientsError.NoActiveAccess, "There is no active access for this veterinarian.");

        repository.Update(link);

        var herds = await herdRepository.ListAsync(cancellationToken);
        foreach (var herd in herds.Where(herd => herd.OwnerId == link.RancherId && herd.VeterinarianId == link.VeterinarianId))
        {
            herd.VeterinarianId = null;
            herdRepository.Update(herd);
        }

        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<VeterinarianClient>.Success(link);
    }

    /// <summary>Finds a link, only if it is addressed to <paramref name="rancherId"/>.</summary>
    private async Task<VeterinarianClient?> FindOwnedAsync(int linkId, int rancherId, CancellationToken cancellationToken)
    {
        var link = await repository.FindByIdAsync(linkId, cancellationToken);
        return link is not null && link.RancherId == rancherId ? link : null;
    }

    private static Result<VeterinarianClient> NotFound() =>
        Result<VeterinarianClient>.Failure(ClientsError.VeterinarianClientNotFound, "Veterinarian request not found.");
}
