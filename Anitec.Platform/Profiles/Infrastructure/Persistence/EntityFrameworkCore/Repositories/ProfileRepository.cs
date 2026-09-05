using Anitec.Platform.Profiles.Domain.Model.Aggregates;
using Anitec.Platform.Profiles.Domain.Model.ValueObjects;
using Anitec.Platform.Profiles.Domain.Repositories;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Anitec.Platform.Profiles.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Profile repository implementation
/// </summary>
/// <param name="context">
///     The database context
/// </param>
public class ProfileRepository(AppDbContext context)
    : BaseRepository<Profile>(context), IProfileRepository
{
    /// <inheritdoc />
    public async Task<Profile?> FindProfileByEmailAsync(EmailAddress email, CancellationToken cancellationToken)
    {
        return await Context.Set<Profile>().FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
    }
}
