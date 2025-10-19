namespace TaskPomodoro.Api.Models;

/// <summary>
/// ポモドーロセッションの種類
/// </summary>
public enum SessionKind
{
    /// <summary>
    /// フォーカス（作業）セッション
    /// </summary>
    Focus = 0,

    /// <summary>
    /// 休憩セッション
    /// </summary>
    Break = 1
}
