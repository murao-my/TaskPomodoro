namespace TaskPomodoro.Api.Constants;

/// <summary>
/// セッション関連のデータベース制約とAPIバリデーションで使用する定数
/// </summary>
public static class SessionConstraints
{
    /// <summary>
    /// 予定時間の最小値（分）
    /// </summary>
    public const int PlannedMinutesMinValue = 1;

    /// <summary>
    /// 予定時間の最大値（分）
    /// </summary>
    public const int PlannedMinutesMaxValue = 120;

    /// <summary>
    /// 実際の時間の最小値（分）
    /// </summary>
    public const int ActualMinutesMinValue = 0;

    /// <summary>
    /// 実際の時間の最大値（分）
    /// </summary>
    public const int ActualMinutesMaxValue = 180;
}
