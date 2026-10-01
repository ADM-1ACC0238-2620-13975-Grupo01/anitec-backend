using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Interfaces.Rest.Resources;

namespace Anitec.Platform.Livestock.Interfaces.Rest.Transform;

public static class CreateAnimalBatchCommandFromResourceAssembler
{
    public static CreateAnimalBatchCommand ToCommandFromResource(CreateAnimalBatchResource resource)
    {
        return new CreateAnimalBatchCommand(resource.Species, resource.Breed, resource.Gender, resource.BirthDate, resource.Weight, resource.Status, resource.HerdId, resource.CorralId!.Value, resource.Quantity, resource.Source, resource.AgeRange, resource.ImageUrl);
    }
}
