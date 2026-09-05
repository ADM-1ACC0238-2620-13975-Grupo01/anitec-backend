using Anitec.Platform.Sanitary.Domain.Model.Commands;
using Anitec.Platform.Sanitary.Interfaces.Rest.Resources;

namespace Anitec.Platform.Sanitary.Interfaces.Rest.Transform;

public static class UpdateHealthEventCommandFromResourceAssembler
{
    public static UpdateHealthEventCommand ToCommandFromResource(int id, CreateHealthEventResource resource)
    {
        return new UpdateHealthEventCommand(id, resource.AnimalId, resource.Type, resource.Date, resource.Description, resource.Veterinarian, resource.Diagnosis, resource.Treatment, resource.Prescription, resource.FollowUp, resource.NextDueDate);
    }
}
