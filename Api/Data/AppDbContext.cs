namespace TaskPomodoro.Api.Data;

using Microsoft.EntityFrameworkCore;
using TaskPomodoro.Api.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Task> Tasks { get; set; } = null!;
    public DbSet<Session> Sessions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ConfigureTaskEntity(modelBuilder);
        ConfigureSessionEntity(modelBuilder);
    }

    private static void ConfigureTaskEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Note).HasMaxLength(1000).IsRequired(false);
            entity.Property(e => e.EstimatedPomos).HasDefaultValue(0).IsRequired(false);
            entity.Property(e => e.IsArchived).HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });
    }

    private static void ConfigureSessionEntity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TaskId).IsRequired();
            entity.Property(e => e.Kind).IsRequired().HasConversion<string>();
            entity.Property(e => e.PlannedMinutes).IsRequired();
            entity.Property(e => e.ActualMinutes).IsRequired(false);
            entity.Property(e => e.StartedAt).IsRequired();
            entity.Property(e => e.EndedAt).IsRequired(false);
            entity.HasOne<Task>().WithMany().HasForeignKey(e => e.TaskId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}