using Anitec.Platform.Analytics.Application.CommandServices;
using Anitec.Platform.Analytics.Domain.Model.Commands;
using Anitec.Platform.Analytics.Domain.Model;
using Anitec.Platform.Analytics.Domain.Model.Entities;
using Anitec.Platform.Analytics.Domain.Repositories;
using Anitec.Platform.Shared.Application.Model;
using Anitec.Platform.Shared.Domain.Repositories;

namespace Anitec.Platform.Analytics.Application.Internal.CommandServices;

public class ReportMetricCommandService(IReportMetricRepository repository, IUnitOfWork unitOfWork)
    : IReportMetricCommandService
{
    public async Task<Result<ReportMetric>> Handle(CreateReportMetricCommand command, CancellationToken cancellationToken)
    {
        var entity = new ReportMetric(command);
        await repository.AddAsync(entity, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<ReportMetric>.Success(entity);
    }

    public async Task<Result<ReportMetric>> Handle(UpdateReportMetricCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result<ReportMetric>.Failure(AnalyticsError.ReportMetricNotFound, "ReportMetric not found.");
        entity.Label = command.Label;
        entity.Value = command.Value;
        entity.Trend = command.Trend;

        repository.Update(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<ReportMetric>.Success(entity);
    }

    public async Task<Result> Handle(DeleteReportMetricCommand command, CancellationToken cancellationToken)
    {
        var entity = await repository.FindByIdAsync(command.Id, cancellationToken);
        if (entity is null) return Result.Failure(AnalyticsError.ReportMetricNotFound, "ReportMetric not found.");
        repository.Remove(entity);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result.Success();
    }
}
