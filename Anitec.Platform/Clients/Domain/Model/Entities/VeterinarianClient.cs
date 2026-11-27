using Anitec.Platform.Clients.Domain.Model.Commands;

namespace Anitec.Platform.Clients.Domain.Model.Entities;

/// <summary>
/// Links a veterinarian with a rancher. The veterinarian asks, the rancher accepts or rejects, and an accepted
/// link can be revoked. Only an accepted link lets the veterinarian see the rancher's data
/// (see <see cref="ClientLinkStatus"/>).
/// </summary>
public class VeterinarianClient
{
    public VeterinarianClient()
    {
        Status = ClientLinkStatus.Pending;
        RequestedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Creates the link from the command; an empty status becomes "Pending", and an "Accepted" one sets <see cref="AcceptedAt"/>.
    /// </summary>
    public VeterinarianClient(CreateVeterinarianClientCommand command)
    {
        VeterinarianId = command.VeterinarianId;
        RancherId = command.RancherId;
        Status = string.IsNullOrWhiteSpace(command.Status) ? ClientLinkStatus.Pending : command.Status;
        RequestedAt = DateTime.UtcNow;
        AcceptedAt = ClientLinkStatus.Is(Status, ClientLinkStatus.Accepted) ? DateTime.UtcNow : null;
    }

    public int Id { get; set; }
    public int VeterinarianId { get; set; }
    public int RancherId { get; set; }
    public string Status { get; set; }
    /// <summary>UTC moment the link was last requested.</summary>
    public DateTime RequestedAt { get; set; }
    /// <summary>UTC moment the link was accepted, or null while it is pending or if it never was.</summary>
    public DateTime? AcceptedAt { get; set; }
    /// <summary>UTC moment the rancher rejected the request or revoked the access, or null if neither happened.</summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>True while the rancher has not answered the request.</summary>
    public bool IsPending => ClientLinkStatus.Is(Status, ClientLinkStatus.Pending);

    /// <summary>True when the veterinarian currently has access to the rancher's data.</summary>
    public bool IsActive => ClientLinkStatus.Is(Status, ClientLinkStatus.Accepted);

    /// <summary>
    /// Opens a new request on a link that was rejected or revoked earlier. Does nothing (returns false)
    /// when the link is pending or active.
    /// </summary>
    public bool Reopen()
    {
        if (IsPending || IsActive) return false;
        Status = ClientLinkStatus.Pending;
        RequestedAt = DateTime.UtcNow;
        AcceptedAt = null;
        ClosedAt = null;
        return true;
    }

    /// <summary>Accepts a pending request, giving the veterinarian access. Returns false if it is not pending.</summary>
    public bool Accept()
    {
        if (!IsPending) return false;
        Status = ClientLinkStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
        ClosedAt = null;
        return true;
    }

    /// <summary>Rejects a pending request without granting access. Returns false if it is not pending.</summary>
    public bool Reject()
    {
        if (!IsPending) return false;
        Status = ClientLinkStatus.Rejected;
        ClosedAt = DateTime.UtcNow;
        return true;
    }

    /// <summary>Withdraws the access of the veterinarian. Returns false if the link is not active.</summary>
    public bool Revoke()
    {
        if (!IsActive) return false;
        Status = ClientLinkStatus.Revoked;
        ClosedAt = DateTime.UtcNow;
        return true;
    }
}
