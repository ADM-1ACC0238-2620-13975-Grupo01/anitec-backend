using Anitec.Platform.Financial.Domain.Model.Entities;
using Anitec.Platform.Financial.Interfaces.Rest.Resources;

namespace Anitec.Platform.Financial.Interfaces.Rest.Transform;


public static class FinancialRecordResourceFromEntityAssembler
{
    public static FinancialRecordResource ToResourceFromEntity(FinancialRecord entity)
    {
        return new FinancialRecordResource(entity.Id, entity.OwnerId, entity.Type, entity.Category, entity.Amount, entity.Date, entity.Description);
    }
}
