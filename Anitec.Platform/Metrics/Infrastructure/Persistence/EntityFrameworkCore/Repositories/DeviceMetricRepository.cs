using Anitec.Platform.Metrics.Domain.Model.Entities;
using Anitec.Platform.Metrics.Domain.Repositories;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

namespace Anitec.Platform.Metrics.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

public class DeviceMetricRepository(AppDbContext context) : BaseRepository<DeviceMetric>(context), IDeviceMetricRepository
{
}
