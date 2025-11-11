using FlexNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexNet.Infrastructure.Data.Configuration;

public class AvatarConfiguration : IEntityTypeConfiguration<Avatar>
{
    public void Configure(EntityTypeBuilder<Avatar> builder)
    {
        builder.Property(a => a.Style).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Personality).HasMaxLength(100).IsRequired();
        builder.Property(a=> a.VoiceEnabled).HasDefaultValue(false);
        builder.Property(a=> a.VoiceSelection).IsRequired().HasMaxLength(50).HasDefaultValue("Default");

        // Cascade delete when a User is deleted from the system/database
        // This ensures that the associated Avatar is also removed
        builder.HasOne(a => a.User)
            .WithOne(u => u.Avatar)
            .HasForeignKey<Avatar>(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}