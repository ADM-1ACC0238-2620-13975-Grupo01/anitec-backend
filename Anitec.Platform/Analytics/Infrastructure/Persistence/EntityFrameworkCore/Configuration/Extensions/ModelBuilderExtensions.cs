using Anitec.Platform.Analytics.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anitec.Platform.Analytics.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyAnalyticsConfiguration(this ModelBuilder builder)
    {
        builder.Entity<ReportMetric>().HasKey(m => m.Id);
        builder.Entity<ReportMetric>().Property(m => m.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<ReportMetric>().Property(m => m.Label).IsRequired().HasMaxLength(80);
        builder.Entity<ReportMetric>().Property(m => m.Value).IsRequired().HasMaxLength(40);
        builder.Entity<ReportMetric>().Property(m => m.Trend).HasMaxLength(80);
    }
}
