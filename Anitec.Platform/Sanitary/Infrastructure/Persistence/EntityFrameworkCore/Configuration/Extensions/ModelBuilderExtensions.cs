using Anitec.Platform.Sanitary.Domain.Model.Entities;
using Anitec.Platform.Livestock.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Anitec.Platform.Sanitary.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplySanitaryConfiguration(this ModelBuilder builder)
    {
        builder.Entity<HealthEvent>().HasKey(h => h.Id);
        builder.Entity<HealthEvent>().Property(h => h.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<HealthEvent>().Property(h => h.Type).IsRequired().HasMaxLength(40);
        builder.Entity<HealthEvent>().Property(h => h.Date)
            .HasConversion(new ValueConverter<DateOnly, DateTime>(
                value => value.ToDateTime(TimeOnly.MinValue),
                value => DateOnly.FromDateTime(value)));
        builder.Entity<HealthEvent>().Property(h => h.NextDueDate)
            .HasConversion(new ValueConverter<DateOnly?, DateTime?>(
                value => value.HasValue ? value.Value.ToDateTime(TimeOnly.MinValue) : null,
                value => value.HasValue ? DateOnly.FromDateTime(value.Value) : null));
        builder.Entity<HealthEvent>().Property(h => h.Description).IsRequired().HasMaxLength(500);
        builder.Entity<HealthEvent>().Property(h => h.Veterinarian).HasMaxLength(80);
        builder.Entity<HealthEvent>()
            .HasOne<Animal>()
            .WithMany()
            .HasForeignKey(h => h.AnimalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
