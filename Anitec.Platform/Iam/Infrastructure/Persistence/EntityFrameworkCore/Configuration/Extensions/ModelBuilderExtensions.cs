using Anitec.Platform.Iam.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace Anitec.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyIamConfiguration(this ModelBuilder builder)
    {
        // IAM Context
        builder.Entity<User>().HasKey(u => u.Id);
        builder.Entity<User>().Property(u => u.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<User>().Property(u => u.Username).IsRequired().HasMaxLength(80);
        builder.Entity<User>().Property(u => u.FullName).IsRequired().HasMaxLength(120);
        builder.Entity<User>().Property(u => u.Role).IsRequired().HasMaxLength(40);
        builder.Entity<User>().Property(u => u.PasswordHash).IsRequired();
    }
    
}
