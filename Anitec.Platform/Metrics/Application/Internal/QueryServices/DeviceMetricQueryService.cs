using Anitec.Platform.Metrics.Application.QueryServices;
using Anitec.Platform.Metrics.Domain.Model.Entities;
using Anitec.Platform.Metrics.Domain.Model.Queries;
using Anitec.Platform.Metrics.Domain.Repositories;

namespace Anitec.Platform.Metrics.Application.Internal.QueryServices;

public class DeviceMetricQueryService(IDeviceMetricRepository repository) : IDeviceMetricQueryService
{
    public async Task<DeviceMetric?> Handle(GetDeviceMetricByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<IEnumerable<DeviceMetric>> Handle(GetAllDeviceMetricsQuery query, CancellationToken cancellationToken)
    {
        return await repository.ListAsync(cancellationToken);
    }
}
