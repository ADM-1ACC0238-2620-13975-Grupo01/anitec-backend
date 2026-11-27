namespace Anitec.Platform.Shared.Domain.Model.Entities;

/// <summary>
///     Marks an entity that clients can edit while offline, so that two edits of the same record made from
///     different places can be told apart instead of the last one silently winning.
/// </summary>
/// <remarks>
///     <see cref="Version" /> starts at 1 and goes up by one every time the record is modified (see
///     <c>VersionedEntityInterceptor</c>). It is also an EF Core concurrency token, so two saves based on the
///     same version cannot both succeed. The API returns it as the <c>ETag</c> header and as <c>version</c>, and
///     checks the <c>If-Match</c> header of updates and deletes against it.
/// </remarks>
public interface IVersionedEntity
{
    /// <summary>Number of the current version of the record, starting at 1.</summary>
    int Version { get; set; }
}
