using Anitec.Platform.Activities.Domain.Model.Commands;
using Anitec.Platform.Activities.Interfaces.Rest.Resources;

namespace Anitec.Platform.Activities.Interfaces.Rest.Transform;

public static class CreateFarmActivityCommandFromResourceAssembler
{
    public static CreateFarmActivityCommand ToCommandFromResource(CreateFarmActivityResource resource)
    {
        return new CreateFarmActivityCommand(resource.OwnerId, resource.VeterinarianId, resource.Title, resource.Type, resource.Date, resource.Priority, resource.Status);
    }
}
