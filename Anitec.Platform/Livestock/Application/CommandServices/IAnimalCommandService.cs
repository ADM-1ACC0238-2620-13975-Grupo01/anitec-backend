using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Domain.Model;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Shared.Application.Model;

namespace Anitec.Platform.Livestock.Application.CommandServices;

public interface IAnimalCommandService
{
    Task<Result<Animal>> Handle(CreateAnimalCommand command, CancellationToken cancellationToken);
    Task<Result<Animal>> Handle(UpdateAnimalCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteAnimalCommand command, CancellationToken cancellationToken);
}
