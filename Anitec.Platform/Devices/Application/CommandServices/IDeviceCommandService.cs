using Anitec.Platform.Devices.Domain.Model.Commands;
using Anitec.Platform.Devices.Domain.Model;
using Anitec.Platform.Devices.Domain.Model.Entities;
using Anitec.Platform.Shared.Application.Model;

namespace Anitec.Platform.Devices.Application.CommandServices;

public interface IDeviceCommandService
{
    Task<Result<Device>> Handle(CreateDeviceCommand command, CancellationToken cancellationToken);
    Task<Result<Device>> Handle(UpdateDeviceCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteDeviceCommand command, CancellationToken cancellationToken);
}
