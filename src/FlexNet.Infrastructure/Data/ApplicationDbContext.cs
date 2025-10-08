using Microsoft.EntityFrameworkCore;
using FlexNet.Domain.Entities;

namespace FlexNet.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<ChatSession> ChatSessions { get; set; }
    public DbSet<Avatar> Avatars { get; set; }
    public DbSet<UserDescription> UserDescriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.PasswordHash)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .IsRequired();

            entity.Property(e => e.IsActive)
                .IsRequired();
        });

        // Configure ChatSession entity
        modelBuilder.Entity<ChatSession>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Summary)
                .HasMaxLength(1000);

            entity.Property(e => e.StartedTime)
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(e => e.ChatSessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Avatar entity
        modelBuilder.Entity<Avatar>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Style)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Personality)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.VoiceSelection)
                .IsRequired()
                .HasMaxLength(100);

            entity.HasOne(e => e.User)
                .WithOne(e => e.Avatar)
                .HasForeignKey<Avatar>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserDescription entity
        modelBuilder.Entity<UserDescription>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Gender)
                .HasMaxLength(20);

            entity.Property(e => e.Education)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Purpose)
                .IsRequired()
                .HasMaxLength(500);

            entity.HasOne(e => e.User)
                .WithOne(e => e.UserDescription)
                .HasForeignKey<UserDescription>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

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