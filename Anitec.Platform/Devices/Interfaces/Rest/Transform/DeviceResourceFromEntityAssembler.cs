using Anitec.Platform.Devices.Domain.Model.Entities;
using Anitec.Platform.Devices.Interfaces.Rest.Resources;

namespace Anitec.Platform.Devices.Interfaces.Rest.Transform;

public static class DeviceResourceFromEntityAssembler
{
    public static DeviceResource ToResourceFromEntity(Device entity)
    {
        return new DeviceResource(entity.Id, entity.Name, entity.Type, entity.SerialNumber, entity.Status, entity.HerdId, entity.AnimalId);
    }
}
