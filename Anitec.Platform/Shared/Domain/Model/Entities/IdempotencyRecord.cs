namespace Anitec.Platform.Shared.Domain.Model.Entities;

/// <summary>
///     Remembers the result of a request sent with an <c>Idempotency-Key</c> header, so that sending it again
///     (a retry after a lost response, for example) returns the same result instead of repeating the effect.
/// </summary>
/// <remarks>
///     A record is created when the request starts, with <see cref="StatusCode" /> 0 ("in progress"), and
///     completed with the response when the request succeeds. The key belongs to the user who sent it.
/// </remarks>
public class IdempotencyRecord
{
    public int Id { get; set; }

    /// <summary>Id of the user who sent the request; the same key from another user is a different record.</summary>
    public int UserId { get; set; }

    /// <summary>The value of the <c>Idempotency-Key</c> header.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>SHA-256 of the method, the path with its query and the body of the request, in hexadecimal.</summary>
    public string RequestHash { get; set; } = string.Empty;

    /// <summary>HTTP status of the stored response, or 0 while the request is still being processed.</summary>
    public int StatusCode { get; set; }

    /// <summary>Body of the stored response.</summary>
    public string? ResponseBody { get; set; }

    /// <summary>Content type of the stored response.</summary>
    public string? ContentType { get; set; }

    /// <summary>Location header of the stored response (for 201 Created).</summary>
    public string? Location { get; set; }

    /// <summary>UTC moment the record was created.</summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>True while the first request with this key has not finished.</summary>
    public bool IsInProgress => StatusCode == 0;
}
