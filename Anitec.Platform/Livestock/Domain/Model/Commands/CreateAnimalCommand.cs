namespace Anitec.Platform.Livestock.Domain.Model.Commands;

public record CreateAnimalCommand(string Tag, string Name, string Species, string Breed, string Gender, DateOnly? BirthDate, decimal Weight, string Status, int HerdId);
