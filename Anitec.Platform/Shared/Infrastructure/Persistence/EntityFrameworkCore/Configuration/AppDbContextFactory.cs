using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Anitec.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
/// Creates the <see cref="AppDbContext"/> for the dotnet-ef tools when migrations are added or applied.
/// It reads the connection string from ANITEC_CONNECTION_STRING and falls back to the local MySQL database.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ANITEC_CONNECTION_STRING")
                               ?? "server=127.0.0.1;port=3306;user=root;password=password;database=anitec-platform";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseMySQL(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
