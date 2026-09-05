using Anitec.Platform.Activities.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Anitec.Platform.Analytics.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Anitec.Platform.Clients.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Anitec.Platform.Devices.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Anitec.Platform.Financial.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Anitec.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Anitec.Platform.Livestock.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Anitec.Platform.Metrics.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Anitec.Platform.Profiles.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Anitec.Platform.Sanitary.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using Anitec.Platform.Subscriptions.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Application database context for the AniTec Platform
/// </summary>
/// <param name="options">
///     The options for the database context
/// </param>
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        // Apply audit timestamp interceptor for all IAuditableEntity implementations
        builder.AddInterceptors(new AuditableEntityInterceptor());
        base.OnConfiguring(builder);
    }

    /// <summary>
    ///     On creating the database model
    /// </summary>
    /// <remarks>
    ///     This method is used to create the database model for the application.
    /// </remarks>
    /// <param name="builder">
    ///     The model builder for the database context
    /// </param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // AniTec Core Contexts
        builder.ApplyLivestockConfiguration();
        builder.ApplySanitaryConfiguration();
        builder.ApplyFinancialConfiguration();
        builder.ApplyActivitiesConfiguration();
        builder.ApplyAnalyticsConfiguration();
        builder.ApplyClientsConfiguration();
        builder.ApplyDevicesConfiguration();
        builder.ApplyMetricsConfiguration();
        builder.ApplySubscriptionsConfiguration();

        // Profiles Context
        builder.ApplyProfilesConfiguration();

        // IAM Context
        builder.ApplyIamConfiguration();

        // General Naming Convention for the database objects
        builder.UseSnakeCaseNamingConvention();
    }
}
