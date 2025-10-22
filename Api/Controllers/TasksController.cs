using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskPomodoro.Api.DTOs;
using TaskPomodoro.Api.Models;
using TaskPomodoro.Api.Repositories;

namespace TaskPomodoro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IRepository<Models.Task> _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TasksController(IRepository<Models.Task> taskRepository, IUnitOfWork unitOfWork)
    {
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TaskResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<TaskResponseDto>>> GetTasks(
        [FromQuery] string? status = null)
    {
        IQueryable<Models.Task> query = _taskRepository.Query();

        // ステータスによるフィルタリング
        switch (status?.ToLower())
        {
            case "active":
                query = query.Where(t => !t.IsArchived);
                break;
            case "archived":
                query = query.Where(t => t.IsArchived);
                break;
            case null:
                // 全タスク（フィルタなし）
                break;
            default:
                return BadRequest("Invalid status parameter. Use 'active' or 'archived'.");
        }

        var tasks = await query.ToListAsync();
        var responseDtos = TaskMapper.ToResponseDtos(tasks);

        return Ok(responseDtos);
    }

    /// <summary>
    /// 新しいタスクを作成する
    /// </summary>
    /// <param name="createDto">タスク作成用のDTO</param>
    /// <returns>作成されたタスク</returns>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponseDto>> CreateTask(TaskCreateDto createDto)
    {
        // バリデーションは[ApiController]により自動実行される

        var task = TaskMapper.FromCreateDto(createDto);
        await _taskRepository.AddAsync(task);
        await _unitOfWork.SaveChangesAsync();

        var responseDto = TaskMapper.ToResponseDto(task);
        return CreatedAtAction(nameof(GetTask), new { id = task.Id }, responseDto);
    }

    /// <summary>
    /// 指定されたIDのタスクを取得する
    /// </summary>
    /// <param name="id">タスクID</param>
    /// <returns>タスク情報</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponseDto>> GetTask(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        var responseDto = TaskMapper.ToResponseDto(task);
        return Ok(responseDto);
    }

    /// <summary>
    /// 指定されたIDのタスクを更新する
    /// </summary>
    /// <param name="id">タスクID</param>
    /// <param name="updateDto">タスク更新用のDTO</param>
    /// <returns>更新されたタスク情報</returns>
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(TaskResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskResponseDto>> UpdateTask(int id, TaskUpdateDto updateDto)
    {
        var existingTask = await _taskRepository.GetByIdAsync(id);
        if (existingTask == null)
        {
            return NotFound();
        }

        // バリデーションは[ApiController]により自動実行される

        var updatedTask = TaskMapper.FromUpdateDto(updateDto, existingTask);
        await _taskRepository.UpdateAsync(updatedTask);
        await _unitOfWork.SaveChangesAsync();

        var responseDto = TaskMapper.ToResponseDto(updatedTask);
        return Ok(responseDto);
    }

    /// <summary>
    /// 指定されたIDのタスクを削除する
    /// </summary>
    /// <param name="id">タスクID</param>
    /// <returns>削除結果</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        await _taskRepository.DeleteAsync(task);
        await _unitOfWork.SaveChangesAsync();

        return NoContent();
    }
}