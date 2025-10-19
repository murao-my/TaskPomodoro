using System.ComponentModel.DataAnnotations;
using TaskPomodoro.Api.Constants;

namespace TaskPomodoro.Api.DTOs;

/// <summary>
/// タスク更新用のDTO
/// </summary>
public class TaskUpdateDto
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(TaskConstraints.TitleMaxLength, ErrorMessage = "Title must be less than {1} characters")]
    public string Title { get; set; } = string.Empty;

    [MaxLength(TaskConstraints.NoteMaxLength, ErrorMessage = "Note must be less than {1} characters")]
    public string? Note { get; set; }

    [Range(TaskConstraints.EstimatedPomosMinValue, TaskConstraints.EstimatedPomosMaxValue, ErrorMessage = "EstimatedPomos must be between {1} and {2}")]
    public int? EstimatedPomos { get; set; }

    public bool IsArchived { get; set; } = false;
}