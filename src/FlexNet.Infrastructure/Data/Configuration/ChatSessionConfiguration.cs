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
    }
} 