using Anitec.Platform.Livestock.Domain.Model.Commands;
using Anitec.Platform.Livestock.Interfaces.Rest.Resources;

namespace Anitec.Platform.Livestock.Interfaces.Rest.Transform;

public static class CreateAnimalCommandFromResourceAssembler
{
    public static CreateAnimalCommand ToCommandFromResource(CreateAnimalResource resource)
    {
        return new CreateAnimalCommand(resource.Tag, resource.Name, resource.Species, resource.Breed, resource.Gender, resource.BirthDate, resource.Weight, resource.Status, resource.HerdId);
    }
}
