using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Domain.Model;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Shared.Application.Model;

namespace Anitec.Platform.Livestock.Application.CommandServices;

public interface IHerdCommandService
{
    Task<Result<Herd>> Handle(CreateHerdCommand command, CancellationToken cancellationToken);
    Task<Result<Herd>> Handle(UpdateHerdCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteHerdCommand command, CancellationToken cancellationToken);
}
