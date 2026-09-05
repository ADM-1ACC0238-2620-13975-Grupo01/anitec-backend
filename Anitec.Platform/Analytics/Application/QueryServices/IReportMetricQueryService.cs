using Anitec.Platform.Analytics.Domain.Model.Entities;
using Anitec.Platform.Analytics.Domain.Model.Queries;

namespace Anitec.Platform.Analytics.Application.QueryServices;

public interface IReportMetricQueryService
{
    Task<ReportMetric?> Handle(GetReportMetricByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<ReportMetric>> Handle(GetAllReportMetricsQuery query, CancellationToken cancellationToken);
}




