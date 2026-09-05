using Anitec.Platform.Livestock.Domain.Model.Commands;

namespace Anitec.Platform.Livestock.Domain.Model.Entities;

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
    public string Owner { get; set; }
    public int OwnerId { get; set; }
    public int? VeterinarianId { get; set; }
    public string MainType { get; set; }
}
