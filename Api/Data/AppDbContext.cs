namespace TaskPomodoro.Api.Data;

using Microsoft.EntityFrameworkCore;
using TaskPomodoro.Api.Models;
using TaskPomodoro.Api.Constants;
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
            entity.Property(e => e => e.Title).IsRequired().HasMaxLength(TaskConstraints.TitleMaxLength);
            entity.Property(e => e.Note).HasMaxLength(TaskConstraints.NoteMaxLength).IsRequired(false);
            entity.Property(e => e.EstimatedPomos).HasDefaultValue(TaskConstraints.EstimatedPomosMinValue).HasMaxLength(TaskConstraints.EstimatedPomosMaxValue).IsRequired(false);
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

            // 外部キー関係の設定
            entity.HasOne(s => s.Task)
                .WithMany()
                .HasForeignKey(s => s.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}