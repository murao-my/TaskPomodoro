using TaskPomodoro.Api.Models;

namespace TaskPomodoro.Api.DTOs;

/// <summary>
/// TaskエンティティとDTO間のマッピングを行う静的クラス
/// </summary>
public static class TaskMapper
{
    /// <summary>
    /// TaskエンティティからTaskResponseDtoにマッピング
    /// </summary>
    /// <param name="task">Taskエンティティ</param>
    /// <returns>TaskResponseDto</returns>
    public static TaskResponseDto ToResponseDto(Task task)
    {
        return new TaskResponseDto
        {
            Id = task.Id,
            Title = task.Title,
            Note = task.Note,
            EstimatedPomos = task.EstimatedPomos,
            IsArchived = task.IsArchived,
            CreatedAt = task.CreatedAt
        };
    }

    /// <summary>
    /// TaskCreateDtoからTaskエンティティにマッピング
    /// </summary>
    /// <param name="createDto">TaskCreateDto</param>
    /// <returns>Taskエンティティ</returns>
    public static Task FromCreateDto(TaskCreateDto createDto)
    {
        return new Task
        {
            Title = createDto.Title,
            Note = createDto.Note,
            EstimatedPomos = createDto.EstimatedPomos,
            IsArchived = false, // 新規作成時は常にfalse
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// TaskUpdateDtoからTaskエンティティにマッピング（既存のエンティティを更新）
    /// </summary>
    /// <param name="updateDto">TaskUpdateDto</param>
    /// <param name="existingTask">既存のTaskエンティティ</param>
    /// <returns>更新されたTaskエンティティ</returns>
    public static Task FromUpdateDto(TaskUpdateDto updateDto, Task existingTask)
    {
        existingTask.Title = updateDto.Title;
        existingTask.Note = updateDto.Note;
        existingTask.EstimatedPomos = updateDto.EstimatedPomos;
        existingTask.IsArchived = updateDto.IsArchived;
        // IdとCreatedAtは変更しない

        return existingTask;
    }

    /// <summary>
    /// TaskエンティティのコレクションからTaskResponseDtoのコレクションにマッピング
    /// </summary>
    /// <param name="tasks">Taskエンティティのコレクション</param>
    /// <returns>TaskResponseDtoのコレクション</returns>
    public static IEnumerable<TaskResponseDto> ToResponseDtos(IEnumerable<Task> tasks)
    {
        return tasks.Select(ToResponseDto);
    }
}