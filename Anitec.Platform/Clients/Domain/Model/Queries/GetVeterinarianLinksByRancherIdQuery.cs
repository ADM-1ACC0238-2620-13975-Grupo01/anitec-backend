namespace Anitec.Platform.Clients.Domain.Model.Queries;

/// <summary>Every request and link addressed to a rancher, optionally only those in one status (US-020).</summary>
public record GetVeterinarianLinksByRancherIdQuery(int RancherId, string? Status = null);
