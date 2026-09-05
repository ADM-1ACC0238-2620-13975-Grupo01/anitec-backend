using Anitec.Platform.Livestock.Domain.Model.Commands;

namespace Anitec.Platform.Livestock.Domain.Model.Entities;

public class Animal
{
    public Animal()
    {
        Tag = string.Empty;
        Name = string.Empty;
        Species = string.Empty;
        Breed = string.Empty;
        Gender = string.Empty;
        Status = string.Empty;
    }

    public Animal(CreateAnimalCommand command)
    {
        Tag = command.Tag;
        Name = command.Name;
        Species = command.Species;
        Breed = command.Breed;
        Gender = command.Gender;
        BirthDate = command.BirthDate;
        Weight = command.Weight;
        Status = command.Status;
        HerdId = command.HerdId;

    }

    public int Id { get; set; }
    public string Tag { get; set; }
    public string Name { get; set; }
    public string Species { get; set; }
    public string Breed { get; set; }
    public string Gender { get; set; }
    public DateOnly? BirthDate { get; set; }
    public decimal Weight { get; set; }
    public string Status { get; set; }
    public int HerdId { get; set; }
}
