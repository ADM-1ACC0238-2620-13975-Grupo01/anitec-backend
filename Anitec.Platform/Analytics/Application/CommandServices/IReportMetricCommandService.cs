using Anitec.Platform.Analytics.Domain.Model.Commands;
using Anitec.Platform.Analytics.Domain.Model;
using Anitec.Platform.Analytics.Domain.Model.Entities;
using Anitec.Platform.Shared.Application.Model;

namespace Anitec.Platform.Analytics.Application.CommandServices;

public interface IReportMetricCommandService
{
    Task<Result<ReportMetric>> Handle(CreateReportMetricCommand command, CancellationToken cancellationToken);
    Task<Result<ReportMetric>> Handle(UpdateReportMetricCommand command, CancellationToken cancellationToken);
    Task<Result> Handle(DeleteReportMetricCommand command, CancellationToken cancellationToken);
}
