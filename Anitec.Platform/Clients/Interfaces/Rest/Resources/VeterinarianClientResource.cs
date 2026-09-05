 namespace Anitec.Platform.Clients.Interfaces.Rest.Resources;

public record VeterinarianClientResource(
    int Id,
    int VeterinarianId,
    int RancherId,
    string RancherName,
    string Status,
    int Herds,
    int Animals,
    DateTime RequestedAt,
    DateTime? AcceptedAt);
