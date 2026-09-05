using Anitec.Platform.Metrics.Domain.Model.Commands;
using Anitec.Platform.Metrics.Domain.Model;
using Anitec.Platform.Metrics.Domain.Model.Entities;
using Anitec.Platform.Shared.Application.Model;

namespace Anitec.Platform.Metrics.Application.CommandServices;

public interface IDeviceMetricCommandService
{
    Task<Result<DeviceMetric>> Handle(CreateDeviceMetricCommand command, CancellationToken cancellationToken);
    Task<Result<DeviceMetric>> Handle(UpdateDeviceMetricCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteDeviceMetricCommand command, CancellationToken cancellationToken);
}
