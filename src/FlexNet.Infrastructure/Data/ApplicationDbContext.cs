using Microsoft.EntityFrameworkCore;
using FlexNet.Domain.Entities;
using FlexNet.Infrastructure.Data.Configuration;

namespace FlexNet.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }
    public DbSet<Avatar> Avatars { get; set; }
    public DbSet<UserDescription> UserDescriptions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply fluent API config 
        modelBuilder.ApplyConfiguration(new AvatarConfiguration());
        modelBuilder.ApplyConfiguration(new ChatMessageConfiguration());
        modelBuilder.ApplyConfiguration(new ChatSessionConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserDescriptionConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed a default admin user with static hash
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                FirstName = "Admin",
                LastName = "User",
                Email = "admin@flexnet.com",
                Role = "Admin",
                PasswordHash = "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lhWy", // Static hash for "admin123"
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                IsActive = true
            }
        );
    }
}