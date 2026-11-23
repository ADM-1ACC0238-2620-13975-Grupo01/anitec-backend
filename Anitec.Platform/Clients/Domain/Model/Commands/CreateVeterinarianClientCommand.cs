namespace Anitec.Platform.Clients.Domain.Model.Commands;


/// <summary>A veterinarian asks a rancher for access. The request starts as "Pending" unless a status is given.</summary>
public record CreateVeterinarianClientCommand(int VeterinarianId, int RancherId, string Status = ClientLinkStatus.Pending);
