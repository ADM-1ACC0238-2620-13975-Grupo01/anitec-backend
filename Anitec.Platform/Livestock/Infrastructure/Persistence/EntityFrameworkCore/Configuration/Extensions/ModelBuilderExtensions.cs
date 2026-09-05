using Anitec.Platform.Livestock.Domain.Model.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Anitec.Platform.Livestock.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyLivestockConfiguration(this ModelBuilder builder)
    {
        builder.Entity<Herd>().HasKey(h => h.Id);
        builder.Entity<Herd>().Property(h => h.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Herd>().Property(h => h.Name).IsRequired().HasMaxLength(80);
        builder.Entity<Herd>().Property(h => h.Location).IsRequired().HasMaxLength(80);
        builder.Entity<Herd>().Property(h => h.Owner).IsRequired().HasMaxLength(80);
        builder.Entity<Herd>().Property(h => h.MainType).IsRequired().HasMaxLength(40);

        builder.Entity<Animal>().HasKey(a => a.Id);
        builder.Entity<Animal>().Property(a => a.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Animal>().Property(a => a.Tag).IsRequired().HasMaxLength(30);
        builder.Entity<Animal>().Property(a => a.Name).IsRequired().HasMaxLength(80);
        builder.Entity<Animal>().Property(a => a.Species).IsRequired().HasMaxLength(40);
        builder.Entity<Animal>().Property(a => a.Breed).IsRequired().HasMaxLength(60);
        builder.Entity<Animal>().Property(a => a.Gender).IsRequired().HasMaxLength(20);
        builder.Entity<Animal>().Property(a => a.BirthDate)
            .HasConversion(new ValueConverter<DateOnly?, DateTime?>(
                value => value.HasValue ? value.Value.ToDateTime(TimeOnly.MinValue) : null,
                value => value.HasValue ? DateOnly.FromDateTime(value.Value) : null));
        builder.Entity<Animal>().Property(a => a.Status).IsRequired().HasMaxLength(30);
        builder.Entity<Animal>()
            .HasOne<Herd>()
            .WithMany()
            .HasForeignKey(a => a.HerdId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
