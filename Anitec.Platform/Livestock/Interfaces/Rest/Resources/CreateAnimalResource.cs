namespace Anitec.Platform.Livestock.Interfaces.Rest.Resources;

public record CreateAnimalResource(string Tag, string Name, string Species, string Breed, string Gender, DateOnly? BirthDate, decimal Weight, string Status, int HerdId);
