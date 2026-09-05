using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Repositories;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Anitec.Platform.Livestock.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class AnimalRepository(AppDbContext context) : BaseRepository<Animal>(context), IAnimalRepository
{
}
