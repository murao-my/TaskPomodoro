using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskPomodoro.Api.DTOs;
using TaskPomodoro.Api.Models;
using TaskPomodoro.Api.Repositories;

namespace TaskPomodoro.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SummaryController : ControllerBase
{
    private readonly IRepository<Session> _sessionRepository;

    public SummaryController(IRepository<Session> sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    /// <summary>
    /// 指定期間のセッションを日次で集計して返す
    /// </summary>
    /// <param name="from">開始日 (YYYY-MM-DD)</param>
    /// <param name="to">終了日 (YYYY-MM-DD)</param>
    [HttpGet]
    [ProducesResponseType(typeof(SummaryResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SummaryResponseDto>> Get([FromQuery] string from, [FromQuery] string to)
    {
        if (!DateOnly.TryParse(from, out var fromDate) || !DateOnly.TryParse(to, out var toDate))
        {
            return BadRequest("Invalid date format. Use YYYY-MM-DD.");
        }
        if (toDate < fromDate)
        {
            return BadRequest("'to' must be greater than or equal to 'from'.");
        }

        var start = fromDate.ToDateTime(TimeOnly.MinValue);
        var end = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue); // exclusive

        var sessions = await _sessionRepository.Query()
            .Where(s => s.StartedAt >= start && s.StartedAt < end)
            .ToListAsync();

        // グルーピングして日次集計
        var groups = sessions
            .GroupBy(s => DateOnly.FromDateTime(s.StartedAt))
            .ToDictionary(g => g.Key, g => new DailySummaryDto
            {
                Date = g.Key,
                FocusMinutes = g.Where(x => x.Kind == SessionKind.Focus)
                                 .Sum(x => x.ActualMinutes ?? (x.EndedAt.HasValue ? (int)(x.EndedAt.Value - x.StartedAt).TotalMinutes : 0)),
                BreakMinutes = g.Where(x => x.Kind == SessionKind.Break)
                                 .Sum(x => x.ActualMinutes ?? (x.EndedAt.HasValue ? (int)(x.EndedAt.Value - x.StartedAt).TotalMinutes : 0)),
                TotalSessions = g.Count(),
                CompletedSessions = g.Count(x => x.EndedAt.HasValue)
            });

        // 期間内の全日付を埋めて返す
        var days = new List<DailySummaryDto>();
        for (var d = fromDate; d <= toDate; d = d.AddDays(1))
        {
            if (groups.TryGetValue(d, out var day))
            {
                days.Add(day);
            }
            else
            {
                days.Add(new DailySummaryDto
                {
                    Date = d,
                    FocusMinutes = 0,
                    BreakMinutes = 0,
                    TotalSessions = 0,
                    CompletedSessions = 0
                });
            }
        }

        var response = new SummaryResponseDto
        {
            From = fromDate,
            To = toDate,
            Days = days
        };

        return Ok(response);
    }
}

