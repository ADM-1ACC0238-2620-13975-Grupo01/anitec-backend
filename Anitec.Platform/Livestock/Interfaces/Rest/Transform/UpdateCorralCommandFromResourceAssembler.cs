using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Interfaces.Rest.Resources;

namespace Anitec.Platform.Livestock.Interfaces.Rest.Transform;

public static class UpdateCorralCommandFromResourceAssembler
{
    public static UpdateCorralCommand ToCommandFromResource(int id, CreateCorralResource resource)
    {
        return new UpdateCorralCommand(id, resource.Name, resource.HerdId);
    }
}
