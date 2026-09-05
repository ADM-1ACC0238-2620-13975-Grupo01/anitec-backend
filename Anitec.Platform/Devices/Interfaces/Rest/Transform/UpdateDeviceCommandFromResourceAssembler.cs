using Anitec.Platform.Devices.Domain.Model.Commands;
using Anitec.Platform.Devices.Interfaces.Rest.Resources;

namespace Anitec.Platform.Devices.Interfaces.Rest.Transform;

public static class UpdateDeviceCommandFromResourceAssembler
{
    public static UpdateDeviceCommand ToCommandFromResource(int id, CreateDeviceResource resource)
    {
        return new UpdateDeviceCommand(id, resource.Name, resource.Type, resource.SerialNumber, resource.Status, resource.HerdId, resource.AnimalId);
    }
}
