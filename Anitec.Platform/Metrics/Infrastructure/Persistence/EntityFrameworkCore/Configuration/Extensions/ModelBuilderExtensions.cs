using Anitec.Platform.Devices.Domain.Model.Entities;
using Anitec.Platform.Metrics.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anitec.Platform.Metrics.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyMetricsConfiguration(this ModelBuilder builder)
    {
        builder.Entity<DeviceMetric>().HasKey(m => m.Id);
        builder.Entity<DeviceMetric>().Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<DeviceMetric>().Property(m => m.Type).IsRequired().HasMaxLength(60);
        builder.Entity<DeviceMetric>().Property(m => m.Unit).IsRequired().HasMaxLength(20);
        builder.Entity<DeviceMetric>().Property(m => m.Value).HasPrecision(12, 2);
        builder.Entity<DeviceMetric>()
            .HasOne<Device>()
            .WithMany()
            .HasForeignKey(m => m.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
