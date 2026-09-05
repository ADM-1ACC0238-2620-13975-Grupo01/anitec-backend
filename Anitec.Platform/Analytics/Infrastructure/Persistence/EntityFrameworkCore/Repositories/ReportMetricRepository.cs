using Anitec.Platform.Analytics.Domain.Model.Entities;
using Anitec.Platform.Analytics.Domain.Repositories;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Anitec.Platform.Analytics.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class ReportMetricRepository(AppDbContext context) : BaseRepository<ReportMetric>(context), IReportMetricRepository
{
}
