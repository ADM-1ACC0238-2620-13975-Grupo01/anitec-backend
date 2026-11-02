using Anitec.Platform.Clients.Domain.Model.Commands;

namespace Anitec.Platform.Clients.Domain.Model.Entities;

/// <summary>
/// Links a veterinarian with a rancher they work for. New links are accepted by default.
/// </summary>
public class VeterinarianClient
{
    public VeterinarianClient()
    {
        Status = "Accepted";
        RequestedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates the link from the command; an empty status becomes "Accepted" and sets <see cref="AcceptedAt"/>.
    /// </summary>
    public VeterinarianClient(CreateVeterinarianClientCommand command)
    {
        VeterinarianId = command.VeterinarianId;
        RancherId = command.RancherId;
        Status = string.IsNullOrWhiteSpace(command.Status) ? "Accepted" : command.Status;
        RequestedAt = DateTime.UtcNow;
        AcceptedAt = Status.Equals("Accepted", StringComparison.OrdinalIgnoreCase) ? DateTime.UtcNow : null;
    }

    public int Id { get; set; }
    public int VeterinarianId { get; set; }
    public int RancherId { get; set; }
    public string Status { get; set; }
    /// <summary>UTC moment the link was requested.</summary>
    public DateTime RequestedAt { get; set; }
    /// <summary>UTC moment the link was accepted, or null while it is pending.</summary>
    public DateTime? AcceptedAt { get; set; }
}
