using Anitec.Platform.Metrics.Domain.Model.Entities;
using Anitec.Platform.Metrics.Interfaces.Rest.Resources;

namespace Anitec.Platform.Metrics.Interfaces.Rest.Transform;

public static class DeviceMetricResourceFromEntityAssembler
{
    public static DeviceMetricResource ToResourceFromEntity(DeviceMetric entity)
    {
        return new DeviceMetricResource(entity.Id, entity.DeviceId, entity.Type, entity.Value, entity.Unit, entity.RecordedAt);
    }
}
