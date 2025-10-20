using TaskPomodoro.Api.Models;

namespace TaskPomodoro.Api.DTOs;

/// <summary>
/// セッション応答用のDTO
/// </summary>
public class SessionResponseDto
{
    public int Id { get; set; }
    public int TaskId { get; set; }
    public SessionKind Kind { get; set; }
    public int PlannedMinutes { get; set; }
    public int? ActualMinutes { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    /// <summary>
    /// セッションの状態（実行中/完了/未完了）
    /// </summary>
    public string Status => EndedAt.HasValue ? "Completed" : "Running";

    /// <summary>
    /// セッションの実行時間（分）
    /// </summary>
    public int? DurationMinutes => ActualMinutes ?? (EndedAt.HasValue ? (int)(EndedAt.Value - StartedAt).TotalMinutes : null);
}
