using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Interfaces.Rest.Resources;

namespace Anitec.Platform.Livestock.Interfaces.Rest.Transform;

public static class CreateCorralCommandFromResourceAssembler
{
    public static CreateCorralCommand ToCommandFromResource(CreateCorralResource resource)
    {
        return new CreateCorralCommand(resource.Name, resource.HerdId);
    }
}
