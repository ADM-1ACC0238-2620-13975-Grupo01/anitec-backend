using Anitec.Platform.Activities.Domain.Model.Commands;
using Anitec.Platform.Activities.Domain.Model;
using Anitec.Platform.Activities.Domain.Model.Entities;
using Anitec.Platform.Shared.Application.Model;

namespace Anitec.Platform.Activities.Application.CommandServices;
// Interfaz del servicio de comandos para gestionar actividades agrícolas, 
// exponiendo métodos asíncronos para crear, actualizar y eliminar registros mediante el patrón Result.
public interface IFarmActivityCommandService
{
    Task<Result<FarmActivity>> Handle(CreateFarmActivityCommand command, CancellationToken cancellationToken);
    Task<Result<FarmActivity>> Handle(UpdateFarmActivityCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteFarmActivityCommand command, CancellationToken cancellationToken);
}
