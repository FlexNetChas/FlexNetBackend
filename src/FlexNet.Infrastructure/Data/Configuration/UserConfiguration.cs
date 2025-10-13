using FlexNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexNet.Infrastructure.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
       builder.Property(u => u.FirstName).IsRequired().HasMaxLength(50); 
       builder.Property(u => u.LastName).IsRequired().HasMaxLength(50);
       builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
       builder.Property(u => u.Role).IsRequired().HasDefaultValue("User");
       builder.Property(u => u.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
       builder.Property(u => u.IsActive).HasDefaultValue(true);
       builder.Property(u => u.PasswordHash).IsRequired().HasMaxLength(100);
       builder.HasIndex(u => u.Email).IsUnique();
    }
    
}