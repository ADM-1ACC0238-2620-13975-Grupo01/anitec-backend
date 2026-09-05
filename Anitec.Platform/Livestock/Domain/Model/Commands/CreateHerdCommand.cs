namespace Anitec.Platform.Livestock.Domain.Model.Commands;

public record CreateHerdCommand(string Name, string Location, string Owner, int OwnerId, int? VeterinarianId, string MainType);
