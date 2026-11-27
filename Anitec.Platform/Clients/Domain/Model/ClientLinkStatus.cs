namespace Anitec.Platform.Clients.Domain.Model;

/// <summary>
///     The states of the link between a veterinarian and a rancher.
///     A veterinarian asks (<see cref="Pending" />); the rancher decides: <see cref="Accepted" /> gives the
///     veterinarian access, <see cref="Rejected" /> closes the request without access. An accepted link can later
///     be withdrawn by the rancher (<see cref="Revoked" />). Only <see cref="Accepted" /> grants access.
/// </summary>
public static class ClientLinkStatus
{
    public const string Pending = "Pending";
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
    public const string Revoked = "Revoked";

    /// <summary>True when <paramref name="status" /> equals <paramref name="expected" />, ignoring case.</summary>
    public static bool Is(string status, string expected) =>
        expected.Equals(status, StringComparison.OrdinalIgnoreCase);
}
