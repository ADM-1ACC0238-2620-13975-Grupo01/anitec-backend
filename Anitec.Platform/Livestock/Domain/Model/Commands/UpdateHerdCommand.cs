namespace Anitec.Platform.Livestock.Domain.Model.Commands;

public record UpdateHerdCommand(int Id, string Name, string Location, string Owner, int OwnerId, int? VeterinarianId, string MainType);
