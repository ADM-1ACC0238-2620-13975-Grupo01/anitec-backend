namespace Anitec.Platform.Activities.Domain.Model.Commands;

public record UpdateFarmActivityCommand(int Id, int? OwnerId, int? VeterinarianId, string Title, string Type, DateOnly Date, string Priority, string Status);
