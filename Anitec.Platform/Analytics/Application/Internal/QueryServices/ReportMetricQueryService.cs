using Anitec.Platform.Analytics.Application.QueryServices;
using Anitec.Platform.Analytics.Domain.Model.Entities;
using Anitec.Platform.Analytics.Domain.Model.Queries;
using Anitec.Platform.Analytics.Domain.Repositories;

namespace Anitec.Platform.Analytics.Application.Internal.QueryServices;

public class ReportMetricQueryService(IReportMetricRepository repository) : IReportMetricQueryService
{
    public async Task<ReportMetric?> Handle(GetReportMetricByIdQuery query, CancellationToken cancellationToken)
    {
        return await repository.FindByIdAsync(query.Id, cancellationToken);
    }

    public async Task<IEnumerable<ReportMetric>> Handle(GetAllReportMetricsQuery query, CancellationToken cancellationToken)
    {
        return await repository.ListAsync(cancellationToken);
    }
}




