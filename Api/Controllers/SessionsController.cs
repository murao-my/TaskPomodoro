using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskPomodoro.Api.DTOs;
using TaskPomodoro.Api.Models;
using TaskPomodoro.Api.Data;

namespace TaskPomodoro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SessionsController : ControllerBase
{
    private readonly IRepository<Session> _sessionRepository;
    private readonly IRepository<Models.Task> _taskRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SessionsController(
        IRepository<Session> sessionRepository,
        IRepository<Models.Task> taskRepository,
        IUnitOfWork unitOfWork)
    {
        _sessionRepository = sessionRepository;
        _taskRepository = taskRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// 新しいセッションを開始する
    /// </summary>
    /// <param name="createDto">セッション作成用のDTO</param>
    /// <returns>作成されたセッション</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SessionResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionResponseDto>> StartSession(SessionCreateDto createDto)
    {
        // バリデーションは[ApiController]により自動実行される

        // タスクの存在確認
        var task = await _taskRepository.GetByIdAsync(createDto.TaskId);
        if (task == null)
        {
            return NotFound($"Task with ID {createDto.TaskId} not found.");
        }

        // 既に実行中のセッションがないかチェック（同じタスクで）
        var existingActiveSession = await _sessionRepository.Query()
            .Where(s => s.TaskId == createDto.TaskId && s.EndedAt == null)
            .FirstOrDefaultAsync();

        if (existingActiveSession != null)
        {
            return Conflict($"There is already an active session for Task ID {createDto.TaskId}.");
        }

        var session = SessionMapper.FromCreateDto(createDto);
        await _sessionRepository.AddAsync(session);
        await _unitOfWork.SaveChangesAsync();

        var responseDto = SessionMapper.ToResponseDto(session);
        return CreatedAtAction(nameof(GetSession), new { id = session.Id }, responseDto);
    }

    /// <summary>
    /// 指定されたIDのセッションを取得する
    /// </summary>
    /// <param name="id">セッションID</param>
    /// <returns>セッション情報</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SessionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SessionResponseDto>> GetSession(int id)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            return NotFound();
        }

        var responseDto = SessionMapper.ToResponseDto(session);
        return Ok(responseDto);
    }

    /// <summary>
    /// 指定されたIDのセッションを完了する
    /// </summary>
    /// <param name="id">セッションID</param>
    /// <param name="updateDto">セッション完了用のDTO</param>
    /// <returns>完了されたセッション情報</returns>
    [HttpPatch("{id}/complete")]
    [ProducesResponseType(typeof(SessionResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SessionResponseDto>> CompleteSession(int id, SessionUpdateDto updateDto)
    {
        var session = await _sessionRepository.GetByIdAsync(id);
        if (session == null)
        {
            return NotFound();
        }

        // 既に完了済みかチェック
        if (session.EndedAt.HasValue)
        {
            return Conflict($"Session with ID {id} is already completed.");
        }

        // バリデーションは[ApiController]により自動実行される

        // セッション完了処理（実際の時間を自動計算）
        var completedSession = SessionMapper.CompleteSession(session, updateDto.EndedAt);

        // 手動でActualMinutesが指定されている場合は上書き
        if (updateDto.ActualMinutes.HasValue)
        {
            completedSession.ActualMinutes = updateDto.ActualMinutes.Value;
        }

        await _sessionRepository.UpdateAsync(completedSession);
        await _unitOfWork.SaveChangesAsync();

        var responseDto = SessionMapper.ToResponseDto(completedSession);
        return Ok(responseDto);
    }

    /// <summary>
    /// 指定された日付のセッション一覧を取得する
    /// </summary>
    /// <param name="date">日付（YYYY-MM-DD形式）</param>
    /// <returns>セッション一覧</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SessionResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<SessionResponseDto>>> GetSessions(
        [FromQuery] string? date = null)
    {
        IQueryable<Session> query = _sessionRepository.Query();

        // 日付によるフィルタリング
        if (!string.IsNullOrEmpty(date))
        {
            if (!DateTime.TryParse(date, out var targetDate))
            {
                return BadRequest("Invalid date format. Use YYYY-MM-DD format.");
            }

            var startOfDay = targetDate.Date;
            var endOfDay = startOfDay.AddDays(1);

            query = query.Where(s => s.StartedAt >= startOfDay && s.StartedAt < endOfDay);
        }

        var sessions = await query
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync();

        var responseDtos = SessionMapper.ToResponseDtos(sessions);
        return Ok(responseDtos);
    }
}
