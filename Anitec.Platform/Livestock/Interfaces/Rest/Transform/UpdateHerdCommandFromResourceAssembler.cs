using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Interfaces.Rest.Resources;

namespace Anitec.Platform.Livestock.Interfaces.Rest.Transform;

public static class UpdateHerdCommandFromResourceAssembler
{
    public static UpdateHerdCommand ToCommandFromResource(int id, CreateHerdResource resource)
    {
        return new UpdateHerdCommand(id, resource.Name, resource.Location, resource.Owner, resource.OwnerId, resource.VeterinarianId, resource.MainType);
    }
}
