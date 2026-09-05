using Anitec.Platform.Metrics.Domain.Model.Commands;
using Anitec.Platform.Metrics.Interfaces.Rest.Resources;

namespace Anitec.Platform.Metrics.Interfaces.Rest.Transform;

public static class UpdateDeviceMetricCommandFromResourceAssembler
{
    public static UpdateDeviceMetricCommand ToCommandFromResource(int id, CreateDeviceMetricResource resource)
    {
        return new UpdateDeviceMetricCommand(id, resource.DeviceId, resource.Type, resource.Value, resource.Unit, resource.RecordedAt);
    }
}
