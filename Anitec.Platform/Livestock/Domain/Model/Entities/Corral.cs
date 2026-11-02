using Anitec.Platform.Livestock.Domain.Model.Commands;

namespace Anitec.Platform.Livestock.Domain.Model.Entities;

/// <summary>
/// A pen inside a herd (farm) where animals are kept.
/// </summary>
public class Corral
{
    public Corral()
    {
        Name = string.Empty;
    }

    public Corral(CreateCorralCommand command)
    {
        Name = command.Name;
        HerdId = command.HerdId;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    /// <summary>Herd the corral belongs to.</summary>
    public int HerdId { get; set; }
}
