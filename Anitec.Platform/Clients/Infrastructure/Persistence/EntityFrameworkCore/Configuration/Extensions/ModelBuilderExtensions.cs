using Anitec.Platform.Clients.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anitec.Platform.Clients.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyClientsConfiguration(this ModelBuilder builder)
    {
        builder.Entity<VeterinarianClient>().HasKey(client => client.Id);
        builder.Entity<VeterinarianClient>().Property(client => client.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<VeterinarianClient>().Property(client => client.Status).IsRequired().HasMaxLength(30);
        builder.Entity<VeterinarianClient>()
            .HasIndex(client => new { client.VeterinarianId, client.RancherId })
            .IsUnique();
    }
}
