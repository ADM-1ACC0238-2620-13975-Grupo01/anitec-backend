namespace Anitec.Platform.Clients.Domain.Model.Commands;

/// <summary>The rancher accepts a pending request of a veterinarian (US-021).</summary>
public record AcceptVeterinarianRequestCommand(int LinkId, int RancherId);

/// <summary>The rancher rejects a pending request of a veterinarian (US-021).</summary>
public record RejectVeterinarianRequestCommand(int LinkId, int RancherId);

/// <summary>The rancher withdraws the access of a veterinarian (US-022).</summary>
public record RevokeVeterinarianAccessCommand(int LinkId, int RancherId);
