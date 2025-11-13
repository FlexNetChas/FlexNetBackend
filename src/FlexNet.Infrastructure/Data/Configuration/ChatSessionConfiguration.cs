using FlexNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlexNet.Infrastructure.Data.Configuration;

public class ChatSessionConfiguration : IEntityTypeConfiguration<ChatSession>
{
    public void Configure(EntityTypeBuilder<ChatSession> builder)
    {
        builder.Property(c => c.Summary).HasMaxLength(maxLength: 1000);
        builder.Property(c => c.StartedTime).HasDefaultValueSql("GETUTCDATE()");

        // Cascade delete when a User is deleted from the system/database
        // This ensures that the associated chat session is also removed
        builder.HasOne(cs => cs.User)
            .WithMany(u => u.ChatSessions)
            .HasForeignKey(cs => cs.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 