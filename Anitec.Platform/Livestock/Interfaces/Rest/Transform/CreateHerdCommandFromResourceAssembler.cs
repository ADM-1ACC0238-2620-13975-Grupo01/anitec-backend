using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Interfaces.Rest.Resources;

namespace Anitec.Platform.Livestock.Interfaces.Rest.Transform;

public static class CreateHerdCommandFromResourceAssembler
{
    public static CreateHerdCommand ToCommandFromResource(CreateHerdResource resource)
    {
        return new CreateHerdCommand(resource.Name, resource.Location, resource.Owner, resource.OwnerId, resource.VeterinarianId, resource.MainType);
    }
}
