using Anitec.Platform.Financial.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Anitec.Platform.Financial.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyFinancialConfiguration(this ModelBuilder builder)
    {
        builder.Entity<FinancialRecord>().HasKey(f => f.Id);
        builder.Entity<FinancialRecord>().Property(f => f.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<FinancialRecord>().Property(f => f.Type).IsRequired().HasMaxLength(20);
        builder.Entity<FinancialRecord>().Property(f => f.Category).IsRequired().HasMaxLength(80);
        builder.Entity<FinancialRecord>().Property(f => f.Amount).HasPrecision(10, 2);
        builder.Entity<FinancialRecord>().Property(f => f.Date)
            .HasConversion(new ValueConverter<DateOnly, DateTime>(
                value => value.ToDateTime(TimeOnly.MinValue),
                value => DateOnly.FromDateTime(value)));
    }
}
