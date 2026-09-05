using Anitec.Platform.Sanitary.Domain.Model.Entities;
using Anitec.Platform.Sanitary.Interfaces.Rest.Resources;

namespace Anitec.Platform.Sanitary.Interfaces.Rest.Transform;

public static class HealthEventResourceFromEntityAssembler
{
    public static HealthEventResource ToResourceFromEntity(HealthEvent entity)
    {
        return new HealthEventResource(entity.Id, entity.AnimalId, entity.Type, entity.Date, entity.Description, entity.Veterinarian, entity.Diagnosis, entity.Treatment, entity.Prescription, entity.FollowUp, entity.NextDueDate);
    }
}