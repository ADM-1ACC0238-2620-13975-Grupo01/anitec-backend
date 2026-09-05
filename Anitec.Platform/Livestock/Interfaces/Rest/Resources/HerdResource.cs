namespace Anitec.Platform.Livestock.Interfaces.Rest.Resources;

public record HerdResource(int Id, string Name, string Location, string Owner, int OwnerId, int? VeterinarianId, string MainType);
