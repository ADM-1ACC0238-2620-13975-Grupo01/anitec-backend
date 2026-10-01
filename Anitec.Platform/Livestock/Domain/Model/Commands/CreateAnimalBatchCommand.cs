namespace Anitec.Platform.Livestock.Domain.Model.Commands;

public record CreateAnimalBatchCommand(string Species, string Breed, string Gender, DateOnly? BirthDate, decimal Weight, string Status, int HerdId, int CorralId, int Quantity, string? Source, string? AgeRange, string? ImageUrl);
