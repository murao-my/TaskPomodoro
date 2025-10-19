using System.ComponentModel.DataAnnotations;

namespace TaskPomodoro.Api.Models;

/// <summary>
/// ポモドーロセッションエンティティ
/// </summary>
public class Session
{
    /// <summary>
    /// セッションID（主キー）
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 関連するタスクID（外部キー）
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    /// セッションの種類（Focus/Break）
    /// </summary>
    public SessionKind Kind { get; set; }

    /// <summary>
    /// 予定時間（分）
    /// </summary>
    public int PlannedMinutes { get; set; }

    /// <summary>
    /// 実際の時間（分）
    /// </summary>
    public int? ActualMinutes { get; set; }

    /// <summary>
    /// 開始時刻
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 終了時刻（完了時に設定）
    /// </summary>
    public DateTime? EndedAt { get; set; }

    /// <summary>
    /// 関連するタスク（ナビゲーションプロパティ）
    /// </summary>
    public Task? Task { get; set; }
}
