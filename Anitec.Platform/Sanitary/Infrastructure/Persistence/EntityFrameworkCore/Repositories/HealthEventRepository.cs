using Anitec.Platform.Sanitary.Domain.Model.Entities;
using Anitec.Platform.Sanitary.Domain.Repositories;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Anitec.Platform.Sanitary.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class HealthEventRepository(AppDbContext context) : BaseRepository<HealthEvent>(context), IHealthEventRepository
{
}
