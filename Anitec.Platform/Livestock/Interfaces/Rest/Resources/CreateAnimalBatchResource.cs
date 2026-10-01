namespace Anitec.Platform.Livestock.Interfaces.Rest.Resources;

public record CreateAnimalBatchResource(string Species, string Breed, string Gender, DateOnly? BirthDate, decimal Weight, string Status, int HerdId, int? CorralId, int Quantity, string? Source, string? AgeRange, string? ImageUrl);
