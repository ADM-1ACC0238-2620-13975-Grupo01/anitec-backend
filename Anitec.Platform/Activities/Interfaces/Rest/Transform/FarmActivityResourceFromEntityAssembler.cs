using Anitec.Platform.Activities.Domain.Model.Entities;
using Anitec.Platform.Activities.Interfaces.Rest.Resources;

namespace Anitec.Platform.Activities.Interfaces.Rest.Transform;

public static class FarmActivityResourceFromEntityAssembler
{
    public static FarmActivityResource ToResourceFromEntity(FarmActivity entity)
    {
        return new FarmActivityResource(entity.Id, entity.OwnerId, entity.VeterinarianId, entity.Title, entity.Type, entity.Date, entity.Priority, entity.Status);
    }
}
