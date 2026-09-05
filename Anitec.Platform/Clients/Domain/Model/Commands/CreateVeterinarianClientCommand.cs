namespace Anitec.Platform.Clients.Domain.Model.Commands;


public record CreateVeterinarianClientCommand(int VeterinarianId, int RancherId, string Status = "Accepted");
