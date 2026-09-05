using Anitec.Platform.Clients.Domain.Model.Entities;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Clients.Domain.Repositories;

public interface IVeterinarianClientRepository : IBaseRepository<VeterinarianClient>
{
    Task<IEnumerable<VeterinarianClient>> FindByVeterinarianIdAsync(int veterinarianId, CancellationToken cancellationToken);
    Task<VeterinarianClient?> FindByVeterinarianIdAndRancherIdAsync(int veterinarianId, int rancherId, CancellationToken cancellationToken);
    Task<bool> ExistsByVeterinarianIdAndRancherIdAsync(int veterinarianId, int rancherId, CancellationToken cancellationToken);
}
