using Anitec.Platform.Activities.Domain.Model.Entities;
using Anitec.Platform.Activities.Domain.Model.Queries;

namespace Anitec.Platform.Activities.Application.QueryServices;
// Interfaz del servicio de consultas (Query Service) para obtener actividades agrícolas,
// exponiendo métodos asíncronos para buscar por identificador único o listar todos los registros.
public interface IFarmActivityQueryService
{
    Task<FarmActivity?> Handle(GetFarmActivityByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<FarmActivity>> Handle(GetAllFarmActivitiesQuery query, CancellationToken cancellationToken);
}




