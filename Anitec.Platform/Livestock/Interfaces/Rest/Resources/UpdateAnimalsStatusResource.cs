namespace Anitec.Platform.Livestock.Interfaces.Rest.Resources;

public record UpdateAnimalsStatusResource(List<int> AnimalIds, string Status);
