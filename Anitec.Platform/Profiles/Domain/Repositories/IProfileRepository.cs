using Anitec.Platform.Profiles.Domain.Model.Aggregates;
using Anitec.Platform.Profiles.Domain.Model.ValueObjects;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Profiles.Domain.Repositories;

/// <summary>
///     Profile repository interface
/// </summary>
public interface IProfileRepository : IBaseRepository<Profile>
{
    /// <summary>
    ///     Find a profile by email
    /// </summary>
    /// <param name="email">
    ///     The <see cref="EmailAddress" /> email address to search for
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The <see cref="Profile" /> if found, otherwise null
    /// </returns>
    Task<Profile?> FindProfileByEmailAsync(EmailAddress email, CancellationToken cancellationToken);
}
