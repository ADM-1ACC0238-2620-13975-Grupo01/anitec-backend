using Anitec.Platform.Livestock.Domain.Model.Entities;
using Anitec.Platform.Livestock.Interfaces.Rest.Resources;

namespace Anitec.Platform.Livestock.Interfaces.Rest.Transform;

public static class AnimalResourceFromEntityAssembler
{
    public static AnimalResource ToResourceFromEntity(Animal entity)
    {
        return new AnimalResource(entity.Id, entity.Tag, entity.Name, entity.Species, entity.Breed, entity.Gender, entity.BirthDate, entity.Weight, entity.Status, entity.HerdId);
    }
}
