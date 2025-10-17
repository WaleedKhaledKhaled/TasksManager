using Microsoft.EntityFrameworkCore;
using TasksManager.Api.Models;

namespace TasksManager.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            // Keep the email tidy and unique so sign in stays predictable
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256);
            entity.Property(u => u.Name).HasMaxLength(200);
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            // Store titles within a friendly length so they display well
            entity.Property(t => t.Title).HasMaxLength(200);
            // These indexes help each user load their task lists without delay
            entity.HasIndex(t => new { t.UserId, t.Status });
            entity.HasIndex(t => new { t.UserId, t.Priority });
            entity.HasIndex(t => new { t.UserId, t.DueDate });
            entity.HasIndex(t => new { t.UserId, t.CreatedAt });
            entity.HasQueryFilter(t => !t.IsDeleted);
            entity.HasOne(t => t.User)
                .WithMany(u => u.Tasks)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
