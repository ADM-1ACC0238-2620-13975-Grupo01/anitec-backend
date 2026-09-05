using Anitec.Platform.Devices.Domain.Model.Commands;
using Anitec.Platform.Devices.Interfaces.Rest.Resources;

namespace Anitec.Platform.Devices.Interfaces.Rest.Transform;

public static class CreateDeviceCommandFromResourceAssembler
{
    public static CreateDeviceCommand ToCommandFromResource(CreateDeviceResource resource)
    {
        return new CreateDeviceCommand(resource.Name, resource.Type, resource.SerialNumber, resource.Status, resource.HerdId, resource.AnimalId);
    }
}
