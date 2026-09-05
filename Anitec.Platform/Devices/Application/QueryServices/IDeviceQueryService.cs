using Anitec.Platform.Devices.Domain.Model.Entities;
using Anitec.Platform.Devices.Domain.Model.Queries;

namespace Anitec.Platform.Devices.Application.QueryServices;

public interface IDeviceQueryService
{
    Task<Device?> Handle(GetDeviceByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<Device>> Handle(GetAllDevicesQuery query, CancellationToken cancellationToken);
}




