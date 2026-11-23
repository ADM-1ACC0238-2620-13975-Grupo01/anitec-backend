using Anitec.Platform.Shared.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;

/// <summary>
///     Raises <see cref="IVersionedEntity.Version" /> by one every time a record is modified, so that each
///     saved change produces a new version. New records keep the initial version.
/// </summary>
public sealed class VersionedEntityInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        BumpVersions(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        BumpVersions(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void BumpVersions(DbContext? context)
    {
        if (context is null) return;

        foreach (var entry in context.ChangeTracker.Entries<IVersionedEntity>().Where(entry => entry.State == EntityState.Modified))
        {
            // The original value is the one the UPDATE is checked against; the current one is what gets stored.
            entry.Entity.Version = entry.Property(item => item.Version).OriginalValue + 1;
        }
    }
}
