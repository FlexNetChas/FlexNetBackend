using FlexNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexNet.Infrastructure.Data.Configuration;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
       builder.HasKey(rt => rt.Id);
       builder.ToTable("RefreshToken");
       builder.Property(rt => rt.Token).IsRequired().HasMaxLength(100);
       builder.HasIndex(rt => rt.Token).IsUnique();
       builder.Property(rt => rt.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
       builder.Property(rt => rt.ExpiresAt);
       builder.Property(rt => rt.UserId).IsRequired();
       builder.Property(rt => rt.IsUsed).HasDefaultValue(false);
       builder.Property(rt => rt.IsRevoked).HasDefaultValue(false);
       builder.Property(rt => rt.UsedAt).IsRequired(false);
       builder.Property(rt => rt.RevokedAt).IsRequired(false);
       builder.HasOne(rt => rt.User).WithMany().HasForeignKey(rt => rt.UserId).OnDelete(DeleteBehavior.Cascade);
       
    }
}