using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Domain.Model;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Shared.Application.Model;

namespace Anitec.Platform.Livestock.Application.CommandServices;

public interface ICorralCommandService
{
    Task<Result<Corral>> Handle(CreateCorralCommand command, CancellationToken cancellationToken);
    Task<Result<Corral>> Handle(UpdateCorralCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteCorralCommand command, CancellationToken cancellationToken);
}
