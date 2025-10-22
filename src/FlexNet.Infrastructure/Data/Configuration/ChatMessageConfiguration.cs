using FlexNet.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace FlexNet.Infrastructure.Data.Configuration;

public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.HasOne(cm => cm.ChatSession) 
            .WithMany(cs => cs.ChatMessages) 
            .HasForeignKey(cm => cm.ChatSessionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.MessageText).IsRequired().HasMaxLength(1000);
        builder.Property(x => x.TimeStamp).HasDefaultValueSql("GETUTCDATE()");
    }
}