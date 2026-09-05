namespace Anitec.Platform.Livestock.Interfaces.Rest.Resources;

public record CreateHerdResource(string Name, string Location, string Owner, int OwnerId, int? VeterinarianId, string MainType);
