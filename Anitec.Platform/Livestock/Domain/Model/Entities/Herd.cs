using Anitec.Platform.Livestock.Domain.Model.Commands;

namespace Anitec.Platform.Livestock.Domain.Model.Entities;

/// <summary>
/// A farm owned by a rancher. It groups corrals and animals and can have a veterinarian in charge.
/// </summary>
public class Herd
{
    public Herd()
    {
        Name = string.Empty;
        Location = string.Empty;
        Owner = string.Empty;
        MainType = string.Empty;
    }

    public Herd(CreateHerdCommand command)
    {
        Name = command.Name;
        Location = command.Location;
        Owner = command.Owner;
        OwnerId = command.OwnerId;
        VeterinarianId = command.VeterinarianId;
        MainType = command.MainType;

    }

    public int Id { get; set; }
    public string Name { get; set; }
    public string Location { get; set; }
    /// <summary>Display name of the owner, kept for listings.</summary>
    public string Owner { get; set; }
    /// <summary>Id of the rancher who owns the farm.</summary>
    public int OwnerId { get; set; }
    /// <summary>Id of the veterinarian in charge, if any.</summary>
    public int? VeterinarianId { get; set; }
    /// <summary>Main kind of livestock raised on the farm.</summary>
    public string MainType { get; set; }
}
