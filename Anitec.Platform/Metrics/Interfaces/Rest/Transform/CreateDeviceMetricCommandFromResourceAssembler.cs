using Anitec.Platform.Metrics.Domain.Model.Commands;
using Anitec.Platform.Metrics.Interfaces.Rest.Resources;

namespace Anitec.Platform.Metrics.Interfaces.Rest.Transform;

public static class CreateDeviceMetricCommandFromResourceAssembler
{
    public static CreateDeviceMetricCommand ToCommandFromResource(CreateDeviceMetricResource resource)
    {
        return new CreateDeviceMetricCommand(resource.DeviceId, resource.Type, resource.Value, resource.Unit, resource.RecordedAt);
    }
}
