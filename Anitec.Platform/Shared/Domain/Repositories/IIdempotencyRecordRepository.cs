using Anitec.Platform.Shared.Domain.Model.Entities;

namespace Anitec.Platform.Shared.Domain.Repositories;

/// <summary>Stores the results of requests sent with an <c>Idempotency-Key</c>.</summary>
public interface IIdempotencyRecordRepository
{
    /// <summary>Finds the record of a user's key, or null if there is none.</summary>
    Task<IdempotencyRecord?> FindAsync(int userId, string key, CancellationToken cancellationToken);

    /// <summary>
    ///     Saves a new record. Returns false when another request already took the same user and key
    ///     (the unique index rejected it), so only one of two simultaneous requests can run.
    /// </summary>
    Task<bool> TryClaimAsync(IdempotencyRecord record, CancellationToken cancellationToken);

    /// <summary>Stores the response of the request that holds the record.</summary>
    Task CompleteAsync(int recordId, int statusCode, string? responseBody, string? contentType, string? location,
        CancellationToken cancellationToken);

    /// <summary>Deletes the record, so that the same key can run again.</summary>
    Task ReleaseAsync(int recordId, CancellationToken cancellationToken);
}
