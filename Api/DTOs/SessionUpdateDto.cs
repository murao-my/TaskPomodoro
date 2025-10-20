using System.ComponentModel.DataAnnotations;
using TaskPomodoro.Api.Constants;

namespace TaskPomodoro.Api.DTOs;

/// <summary>
/// セッション更新用のDTO（主に完了時に使用）
/// </summary>
public class SessionUpdateDto
{
    [Range(SessionConstraints.ActualMinutesMinValue, SessionConstraints.ActualMinutesMaxValue,
           ErrorMessage = "ActualMinutes must be between {1} and {2}")]
    public int? ActualMinutes { get; set; }

    public DateTime? EndedAt { get; set; }
}
