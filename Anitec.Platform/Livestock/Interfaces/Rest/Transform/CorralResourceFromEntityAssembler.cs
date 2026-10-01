using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Interfaces.Rest.Resources;

namespace Anitec.Platform.Livestock.Interfaces.Rest.Transform;

public static class CorralResourceFromEntityAssembler
{
    public static CorralResource ToResourceFromEntity(Corral entity)
    {
        return new CorralResource(entity.Id, entity.Name, entity.HerdId);
    }
}
