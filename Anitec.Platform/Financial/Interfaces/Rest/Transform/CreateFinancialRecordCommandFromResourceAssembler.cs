using Anitec.Platform.Financial.Domain.Model.Commands;
using Anitec.Platform.Financial.Interfaces.Rest.Resources;

namespace Anitec.Platform.Financial.Interfaces.Rest.Transform;


public static class CreateFinancialRecordCommandFromResourceAssembler
{
    public static CreateFinancialRecordCommand ToCommandFromResource(CreateFinancialRecordResource resource)
    {
        return new CreateFinancialRecordCommand(resource.OwnerId, resource.Type, resource.Category, resource.Amount, resource.Date, resource.Description);
    }
}
