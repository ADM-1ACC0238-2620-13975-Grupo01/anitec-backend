using Anitec.Platform.Shared.Domain.Model.Entities;
using Anitec.Platform.Shared.Domain.Repositories;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>EF Core store for the results of requests sent with an <c>Idempotency-Key</c>.</summary>
public class IdempotencyRecordRepository(AppDbContext context) : IIdempotencyRecordRepository
{
    /// <inheritdoc />
    public async Task<IdempotencyRecord?> FindAsync(int userId, string key, CancellationToken cancellationToken)
    {
        // Read without tracking: another request may complete the record while this one looks at it.
        return await context.Set<IdempotencyRecord>()
            .AsNoTracking()
            .FirstOrDefaultAsync(record => record.UserId == userId && record.Key == key, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> TryClaimAsync(IdempotencyRecord record, CancellationToken cancellationToken)
    {
        await context.Set<IdempotencyRecord>().AddAsync(record, cancellationToken);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            // The unique index (user, key) refused it: another request holds the key.
            context.Entry(record).State = EntityState.Detached;
            return false;
        }
    }

    /// <inheritdoc />
    public async Task CompleteAsync(int recordId, int statusCode, string? responseBody, string? contentType,
        string? location, CancellationToken cancellationToken)
    {
        var record = await context.Set<IdempotencyRecord>().FindAsync([recordId], cancellationToken);
        if (record is null) return;

        record.StatusCode = statusCode;
        record.ResponseBody = responseBody;
        record.ContentType = contentType;
        record.Location = location;
        await context.SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task ReleaseAsync(int recordId, CancellationToken cancellationToken)
    {
        // The request that failed may have left changes half-tracked; drop them, then delete the record by its key.
        context.ChangeTracker.Clear();
        context.Set<IdempotencyRecord>().Remove(new IdempotencyRecord { Id = recordId });
        await context.SaveChangesAsync(cancellationToken);
    }
}
