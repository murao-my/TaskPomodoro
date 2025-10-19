namespace TaskPomodoro.Api.DTOs;

/// <summary>
/// タスク応答用のDTO
/// </summary>
public class TaskResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Note { get; set; }
    public int? EstimatedPomos { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
}