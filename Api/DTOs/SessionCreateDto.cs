using System.ComponentModel.DataAnnotations;
using TaskPomodoro.Api.Constants;
using TaskPomodoro.Api.Models;

namespace TaskPomodoro.Api.DTOs;

/// <summary>
/// セッション作成用のDTO
/// </summary>
public class SessionCreateDto
{
    [Required(ErrorMessage = "TaskId is required")]
    public int TaskId { get; set; }

    [Required(ErrorMessage = "Kind is required")]
    public SessionKind Kind { get; set; }

    [Required(ErrorMessage = "PlannedMinutes is required")]
    [Range(SessionConstraints.PlannedMinutesMinValue, SessionConstraints.PlannedMinutesMaxValue,
           ErrorMessage = "PlannedMinutes must be between {1} and {2}")]
    public int PlannedMinutes { get; set; }

    [Required(ErrorMessage = "StartedAt is required")]
    public DateTime StartedAt { get; set; }
}
