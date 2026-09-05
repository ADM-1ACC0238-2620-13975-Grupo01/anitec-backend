using Anitec.Platform.Devices.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace Anitec.Platform.Devices.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyDevicesConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Device>().HasKey(d => d.Id);
        builder.Entity<Device>().Property(d => d.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Device>().Property(d => d.Name).IsRequired().HasMaxLength(80);
        builder.Entity<Device>().Property(d => d.Type).IsRequired().HasMaxLength(60);
        builder.Entity<Device>().Property(d => d.SerialNumber).IsRequired().HasMaxLength(80);
        builder.Entity<Device>().Property(d => d.Status).IsRequired().HasMaxLength(30);
        builder.Entity<Device>()
            .HasOne<Herd>()
            .WithMany()
            .HasForeignKey(d => d.HerdId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.Entity<Device>()
            .HasOne<Animal>()
            .WithMany()
            .HasForeignKey(d => d.AnimalId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
