using Anitec.Platform.Metrics.Domain.Model.Entities;
using Anitec.Platform.Metrics.Domain.Model.Queries;

namespace Anitec.Platform.Metrics.Application.QueryServices;

public interface IDeviceMetricQueryService
{
    Task<DeviceMetric?> Handle(GetDeviceMetricByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<DeviceMetric>> Handle(GetAllDeviceMetricsQuery query, CancellationToken cancellationToken);
}
