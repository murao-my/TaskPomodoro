using System.ComponentModel.DataAnnotations;
namespace TaskPomodoro.Api.Models;

public class Task
{

    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int? EstimatedPomos { get; set; }
    public bool IsArchived { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}