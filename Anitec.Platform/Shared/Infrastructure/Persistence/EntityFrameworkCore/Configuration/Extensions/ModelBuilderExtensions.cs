using Anitec.Platform.Shared.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

/// <summary>
///     Model builder extensions for the database context
/// </summary>
public static class ModelBuilderExtensions
{
    /// <summary>
    ///     Configures the entities shared by every bounded context, such as the stored results of idempotent requests.
    /// </summary>
    public static void ApplySharedConfiguration(this ModelBuilder builder)
    {
        builder.Entity<IdempotencyRecord>().HasKey(record => record.Id);
        builder.Entity<IdempotencyRecord>().Property(record => record.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<IdempotencyRecord>().Property(record => record.Key).IsRequired().HasMaxLength(100);
        builder.Entity<IdempotencyRecord>().Property(record => record.RequestHash).IsRequired().HasMaxLength(64);
        builder.Entity<IdempotencyRecord>().Property(record => record.ContentType).HasMaxLength(200);
        builder.Entity<IdempotencyRecord>().Property(record => record.Location).HasMaxLength(500);
        builder.Entity<IdempotencyRecord>().Ignore(record => record.IsInProgress);
        // A key belongs to one user; the unique index is what keeps two simultaneous requests apart.
        builder.Entity<IdempotencyRecord>().HasIndex(record => new { record.UserId, record.Key }).IsUnique();
    }

    /// <summary>
    ///     Makes the version of every <see cref="IVersionedEntity" /> a concurrency token, so a save based on an
    ///     outdated version fails instead of overwriting a newer one. Call it after the entities are configured.
    /// </summary>
    public static void ApplyVersioning(this ModelBuilder builder)
    {
        foreach (var entityType in builder.Model.GetEntityTypes()
                     .Where(type => typeof(IVersionedEntity).IsAssignableFrom(type.ClrType)).ToList())
            builder.Entity(entityType.ClrType).Property(nameof(IVersionedEntity.Version)).IsConcurrencyToken();
    }

    /// <summary>
    ///     Use snake case naming convention for the database context
    /// </summary>
    /// <param name="builder">
    ///     The model builder for the database context
    /// </param>
    public static void UseSnakeCaseNamingConvention(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes())
        {
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName)) entity.SetTableName(tableName.ToPlural().ToSnakeCase());

            foreach (var property in entity.GetProperties())
                property.SetColumnName(property.GetColumnName().ToSnakeCase());

            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (!string.IsNullOrEmpty(keyName)) key.SetName(keyName.ToSnakeCase());
            }

            foreach (var foreignKey in entity.GetForeignKeys())
            {
                var foreignKeyName = foreignKey.GetConstraintName();
                if (!string.IsNullOrEmpty(foreignKeyName)) foreignKey.SetConstraintName(foreignKeyName.ToSnakeCase());
            }

            foreach (var index in entity.GetIndexes())
            {
                var indexDatabaseName = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(indexDatabaseName)) index.SetDatabaseName(indexDatabaseName.ToSnakeCase());
            }
        }
    }
}
