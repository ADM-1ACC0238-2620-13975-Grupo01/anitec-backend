using Anitec.Platform.Livestock.Domain.Model.Commands;

namespace Anitec.Platform.Livestock.Domain.Model.Entities;

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
    public int HerdId { get; set; }
}
