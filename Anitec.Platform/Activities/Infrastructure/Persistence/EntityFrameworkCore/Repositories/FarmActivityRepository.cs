using Anitec.Platform.Activities.Domain.Model.Entities;
using Anitec.Platform.Activities.Domain.Repositories;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Anitec.Platform.Activities.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class FarmActivityRepository(AppDbContext context) : BaseRepository<FarmActivity>(context), IFarmActivityRepository
{
}
