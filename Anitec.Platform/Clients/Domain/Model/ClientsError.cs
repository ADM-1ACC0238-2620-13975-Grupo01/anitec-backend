namespace Anitec.Platform.Clients.Domain.Model;


public enum ClientsError
{
    VeterinarianClientNotFound,
    VeterinarianClientAlreadyExists,
    /// <summary>The veterinarian already asked and the rancher has not answered yet.</summary>
    RequestAlreadyPending,
    /// <summary>The request was already answered, so it can no longer be accepted or rejected.</summary>
    RequestNotPending,
    /// <summary>The rancher tried to revoke a veterinarian who has no active access.</summary>
    NoActiveAccess,
    /// <summary>The user a request is addressed to does not exist or is not a rancher.</summary>
    RancherNotFound
}
