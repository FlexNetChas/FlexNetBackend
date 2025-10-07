using FlexNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexNet.Infrastructure.Data.Configuration;

public class UserDescriptionConfiguration : IEntityTypeConfiguration<UserDescription>
{

    public void Configure(EntityTypeBuilder<UserDescription> builder)
    {
        builder.Property(u => u.Education).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Purpose).IsRequired().HasMaxLength(50);
        
    }
}