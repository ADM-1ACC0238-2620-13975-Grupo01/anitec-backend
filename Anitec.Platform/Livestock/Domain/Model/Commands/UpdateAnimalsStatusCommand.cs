namespace Anitec.Platform.Livestock.Domain.Model.Commands;

public record UpdateAnimalsStatusCommand(List<int> AnimalIds, string Status);
