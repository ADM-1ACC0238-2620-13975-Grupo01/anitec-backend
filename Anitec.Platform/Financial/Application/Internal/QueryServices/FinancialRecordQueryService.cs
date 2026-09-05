using Anitec.Platform.Financial.Application.QueryServices;
using Anitec.Platform.Financial.Domain.Model.Entities;
using Anitec.Platform.Financial.Domain.Model.Queries;
using Anitec.Platform.Financial.Domain.Repositories;

namespace Anitec.Platform.Financial.Application.Internal.QueryServices;

public class FinancialRecordQueryService(IFinancialRecordRepository repository) : IFinancialRecordQueryService
{
    public async Task<FinancialRecord?> Handle(GetFinancialRecordByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<IEnumerable<FinancialRecord>> Handle(GetAllFinancialRecordsQuery query, CancellationToken cancellationToken)
    {
        return await repository.ListAsync(cancellationToken);
    }
}




