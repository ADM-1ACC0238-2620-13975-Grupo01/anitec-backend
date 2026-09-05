using Anitec.Platform.Clients.Domain.Model.Entities;
using Anitec.Platform.Clients.Domain.Repositories;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Anitec.Platform.Clients.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class VeterinarianClientRepository(AppDbContext context)
    : BaseRepository<VeterinarianClient>(context), IVeterinarianClientRepository
{
    public async Task<IEnumerable<VeterinarianClient>> FindByVeterinarianIdAsync(
        int veterinarianId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<VeterinarianClient>()
            .Where(client => client.VeterinarianId == veterinarianId)
            .ToListAsync(cancellationToken);
    }

    public async Task<VeterinarianClient?> FindByVeterinarianIdAndRancherIdAsync(
        int veterinarianId,
        int rancherId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<VeterinarianClient>()
            .FirstOrDefaultAsync(
                client => client.VeterinarianId == veterinarianId && client.RancherId == rancherId,
                cancellationToken);
    }

    public async Task<bool> ExistsByVeterinarianIdAndRancherIdAsync(
        int veterinarianId,
        int rancherId,
        CancellationToken cancellationToken)
    {
        return await Context.Set<VeterinarianClient>()
            .AnyAsync(
                client => client.VeterinarianId == veterinarianId && client.RancherId == rancherId,
                cancellationToken);
    }
}
