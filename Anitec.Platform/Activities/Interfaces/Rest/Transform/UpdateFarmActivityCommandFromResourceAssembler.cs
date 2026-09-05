using Anitec.Platform.Activities.Domain.Model.Commands;
using Anitec.Platform.Activities.Interfaces.Rest.Resources;

namespace Anitec.Platform.Activities.Interfaces.Rest.Transform;

public static class UpdateFarmActivityCommandFromResourceAssembler
{
    public static UpdateFarmActivityCommand ToCommandFromResource(int id, CreateFarmActivityResource resource)
    {
        return new UpdateFarmActivityCommand(id, resource.OwnerId, resource.VeterinarianId, resource.Title, resource.Type, resource.Date, resource.Priority, resource.Status);
    }
}
