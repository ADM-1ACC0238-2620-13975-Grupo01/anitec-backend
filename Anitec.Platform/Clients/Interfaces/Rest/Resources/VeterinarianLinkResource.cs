namespace Anitec.Platform.Clients.Interfaces.Rest.Resources;

/// <summary>
/// A request or link between a veterinarian and a rancher, as the rancher sees it: who asked (identity of the
/// professional), in which state it is and when it changed. <c>Status</c> is Pending, Accepted, Rejected or Revoked.
/// </summary>
public record VeterinarianLinkResource(
    int Id,
    int VeterinarianId,
    string VeterinarianName,
    string VeterinarianUsername,
    int RancherId,
    string Status,
    DateTime RequestedAt,
    DateTime? AcceptedAt,
    DateTime? ClosedAt);
