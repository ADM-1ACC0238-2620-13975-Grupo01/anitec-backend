using Anitec.Platform.Sanitary.Domain.Model.Commands;
using Anitec.Platform.Sanitary.Interfaces.Rest.Resources;

namespace Anitec.Platform.Sanitary.Interfaces.Rest.Transform;

public static class CreateHealthEventCommandFromResourceAssembler
{
    public static CreateHealthEventCommand ToCommandFromResource(CreateHealthEventResource resource)
    {
        return new CreateHealthEventCommand(resource.AnimalId, resource.Type, resource.Date, resource.Description, resource.Veterinarian, resource.Diagnosis, resource.Treatment, resource.Prescription, resource.FollowUp, resource.NextDueDate);
    }
}
