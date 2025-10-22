namespace TaskPomodoro.Api.DTOs;

/// <summary>
/// 指定期間のサマリー応答
/// </summary>
public class SummaryResponseDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public IReadOnlyList<DailySummaryDto> Days { get; set; } = Array.Empty<DailySummaryDto>();
}

/// <summary>
/// 1日分の集計
/// </summary>
public class DailySummaryDto
{
    public DateOnly Date { get; set; }
    public int FocusMinutes { get; set; }
    public int BreakMinutes { get; set; }
    public int TotalSessions { get; set; }
    public int CompletedSessions { get; set; }
}

