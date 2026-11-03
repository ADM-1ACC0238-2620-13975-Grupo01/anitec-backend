using Anitec.Platform.Activities.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Anitec.Platform.Activities.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
// Clase de extensión para ModelBuilder que configura el mapeo entidad-relación de Entity Framework Core
// para la entidad FarmActivity, definiendo llaves primarias, restricciones de nulabilidad, longitudes máximas
// y conversiones de tipos para la fecha.
public static class ModelBuilderExtensions
{
    public static void ApplyActivitiesConfiguration(this ModelBuilder builder)
    {
        builder.Entity<FarmActivity>().HasKey(a => a.Id);
        builder.Entity<FarmActivity>().Property(a => a.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<FarmActivity>().Property(a => a.Title).IsRequired().HasMaxLength(120);
        builder.Entity<FarmActivity>().Property(a => a.Type).IsRequired().HasMaxLength(40);
        builder.Entity<FarmActivity>().Property(a => a.Date)
            .HasConversion(new ValueConverter<DateOnly, DateTime>(
                value => value.ToDateTime(TimeOnly.MinValue),
                value => DateOnly.FromDateTime(value)));
        builder.Entity<FarmActivity>().Property(a => a.Priority).IsRequired().HasMaxLength(20);
        builder.Entity<FarmActivity>().Property(a => a.Status).IsRequired().HasMaxLength(30);
    }
}
