using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Interfaces.Rest.Resources;

namespace Anitec.Platform.Livestock.Interfaces.Rest.Transform;

public static class HerdResourceFromEntityAssembler
{
    public static HerdResource ToResourceFromEntity(Herd entity)
    {
        return new HerdResource(entity.Id, entity.Name, entity.Location, entity.Owner, entity.OwnerId, entity.VeterinarianId, entity.MainType);
    }
}
