using Anitec.Platform.Activities.Domain.Model.Entities;
using Anitec.Platform.Activities.Domain.Repositories;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Anitec.Platform.Activities.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
// Repositorio concreto para la entidad FarmActivity que hereda de la clase base genérica BaseRepository 
// e implementa el contrato específico IFarmActivityRepository utilizando el contexto de base de datos de la aplicación.
public class FarmActivityRepository(AppDbContext context) : BaseRepository<FarmActivity>(context), IFarmActivityRepository
{
}
