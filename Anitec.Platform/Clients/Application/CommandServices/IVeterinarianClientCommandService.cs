using Anitec.Platform.Clients.Domain.Model.Commands;
using Anitec.Platform.Clients.Domain.Model.Entities;
using Anitec.Platform.Shared.Application.Model;

namespace Anitec.Platform.Clients.Application.CommandServices;

public interface IVeterinarianClientCommandService
{
    Task<Result<VeterinarianClient>> Handle(CreateVeterinarianClientCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteVeterinarianClientCommand command, CancellationToken cancellationToken);
}
