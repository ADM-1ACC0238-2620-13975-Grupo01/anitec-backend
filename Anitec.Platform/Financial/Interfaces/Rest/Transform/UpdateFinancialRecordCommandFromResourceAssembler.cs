using Anitec.Platform.Financial.Domain.Model.Commands;
using Anitec.Platform.Financial.Interfaces.Rest.Resources;

namespace Anitec.Platform.Financial.Interfaces.Rest.Transform;


public static class UpdateFinancialRecordCommandFromResourceAssembler
{
    public static UpdateFinancialRecordCommand ToCommandFromResource(int id, CreateFinancialRecordResource resource)
    {
        return new UpdateFinancialRecordCommand(id, resource.OwnerId, resource.Type, resource.Category, resource.Amount, resource.Date, resource.Description);
    }
}
