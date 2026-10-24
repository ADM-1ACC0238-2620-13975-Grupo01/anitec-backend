using Anitec.Platform.Activities.Domain.Model.Entities;
using Anitec.Platform.Devices.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Model.Entities;

namespace Anitec.Platform.Shared.Application.Internal.Authorization;

/// <summary>
///     What the signed-in user is allowed to see.
///     A rancher sees what they own. A veterinarian sees the herds of their accepted clients and the
///     herds that name them as veterinarian; animals, health events and devices follow from those herds.
///     These are the same rules the mobile app applies on its side.
/// </summary>
public sealed class AccessScope(
    int userId,
    bool isVeterinarian,
    IReadOnlySet<int> clientRancherIds,
    IReadOnlySet<int> herdIds,
    IReadOnlySet<int> animalIds)
{
    /// <summary>Id of the signed-in user.</summary>
    public int UserId { get; } = userId;

    /// <summary>True when the signed-in user is a veterinarian.</summary>
    public bool IsVeterinarian { get; } = isVeterinarian;

    /// <summary>True when <paramref name="herd" /> is visible to the user.</summary>
    public bool CanSeeHerd(Herd herd) => herdIds.Contains(herd.Id);

    /// <summary>True when the herd with this id is visible to the user.</summary>
    public bool CanSeeHerdId(int herdId) => herdIds.Contains(herdId);

    /// <summary>True when the animal with this id belongs to a visible herd.</summary>
    public bool CanSeeAnimalId(int animalId) => animalIds.Contains(animalId);

    /// <summary>True when the rancher with this id is the user (rancher) or one of their clients (veterinarian).</summary>
    public bool CanSeeRancher(int? rancherId) =>
        rancherId is { } id && (IsVeterinarian ? clientRancherIds.Contains(id) : id == UserId);

    /// <summary>A rancher sees activities about them; a veterinarian sees those they created or about their clients.</summary>
    public bool CanSeeActivity(FarmActivity activity) =>
        IsVeterinarian
            ? activity.VeterinarianId == UserId || CanSeeRancher(activity.OwnerId)
            : activity.OwnerId == UserId;

    /// <summary>
    ///     Who a new or edited activity may belong to: a rancher can only create their own, and a
    ///     veterinarian can only create their own for one of their clients (or for nobody in particular).
    /// </summary>
    public bool CanWriteActivity(int? ownerId, int? veterinarianId) =>
        IsVeterinarian
            ? veterinarianId == UserId && (ownerId is null || CanSeeRancher(ownerId))
            : ownerId == UserId;

    /// <summary>A device is visible when it hangs from a visible herd or animal.</summary>
    public bool CanSeeDevice(Device device) =>
        device.HerdId is { } herdId && herdIds.Contains(herdId)
        || device.AnimalId is { } animalId && animalIds.Contains(animalId);
}
