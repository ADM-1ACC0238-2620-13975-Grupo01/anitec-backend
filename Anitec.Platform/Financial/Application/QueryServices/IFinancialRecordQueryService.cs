using Anitec.Platform.Financial.Domain.Model.Entities;
using Anitec.Platform.Financial.Domain.Model.Queries;

namespace Anitec.Platform.Financial.Application.QueryServices;

public interface IFinancialRecordQueryService
{
    Task<FinancialRecord?> Handle(GetFinancialRecordByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<FinancialRecord>> Handle(GetAllFinancialRecordsQuery query, CancellationToken cancellationToken);
}




