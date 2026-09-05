namespace Anitec.Platform.Activities.Interfaces.Rest.Resources;

public record CreateFarmActivityResource(int? OwnerId, int? VeterinarianId, string Title, string Type, DateOnly Date, string Priority, string Status);
