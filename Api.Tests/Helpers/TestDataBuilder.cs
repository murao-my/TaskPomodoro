using TaskPomodoro.Api.Models;
using TaskPomodoro.Api.DTOs;

namespace TaskPomodoro.Api.Tests.Helpers;

public class TestDataBuilder
{
    public static Models.Task CreateTask(
        int id = 1,
        string title = "Test Task",
        string? note = null,
        int? estimatedPomos = 3,
        bool isArchived = false,
        DateTime? createdAt = null
    )
    {
        return new Models.Task { Id = id, Title = title, Note = note, EstimatedPomos = estimatedPomos, IsArchived = isArchived, CreatedAt = createdAt ?? DateTime.UtcNow };
    }

    public static Models.Session CreateSession(
        int id = 1,
        int taskId = 1,
        SessionKind kind = SessionKind.Focus,
        int plannedMinutes = 25,
        int? actualMinutes = null,
        DateTime? startedAt = null,
        DateTime? endedAt = null
    )
    {
        return new Models.Session { Id = id, TaskId = taskId, Kind = kind, PlannedMinutes = plannedMinutes, ActualMinutes = actualMinutes, StartedAt = startedAt ?? DateTime.UtcNow, EndedAt = endedAt };
    }
    public static TaskCreateDto CreateTaskCreateDto(
        string title = "Test Task",
        string? note = null,
        int? estimatedPomos = 3
    )
    {
        return new TaskCreateDto { Title = title, Note = note, EstimatedPomos = estimatedPomos };
    }
    public static SessionCreateDto CreateSessionCreateDto(
        int taskId = 1,
        SessionKind kind = SessionKind.Focus,
        int plannedMinutes = 25,
        DateTime? startedAt = null
    )
    {
        return new SessionCreateDto { TaskId = taskId, Kind = kind, PlannedMinutes = plannedMinutes, StartedAt = startedAt ?? DateTime.UtcNow };
    }
}