using System.ComponentModel.DataAnnotations;
namespace TaskPomodoro.Api.Models;

public class Task
{
    [Key]
    public int Id { get; set; }
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(100, ErrorMessage = "Title must be less than 100 characters")]
    public string Title { get; set; } = string.Empty;
    [MaxLength(1000, ErrorMessage = "Note must be less than 1000 characters")]
    public string? Note { get; set; }
    [Range(1, 100, ErrorMessage = "EstimatedPomos must be between 1 and 100")]
    public int? EstimatedPomos { get; set; }
    public bool IsArchived { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}