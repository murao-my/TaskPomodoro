namespace TaskPomodoro.Api.Constants;

/// <summary>
/// データベース制約とAPIバリデーションで使用する定数
/// </summary>
public static class TaskConstraints
{
    /// <summary>
    /// タスクタイトルの最大文字数
    /// </summary>
    public const int TitleMaxLength = 100;

    /// <summary>
    /// タスクメモの最大文字数
    /// </summary>
    public const int NoteMaxLength = 1000;

    /// <summary>
    /// 見積りポモ数の最小値
    /// </summary>
    public const int EstimatedPomosMinValue = 1;

    /// <summary>
    /// 見積りポモ数の最大値
    /// </summary>
    public const int EstimatedPomosMaxValue = 100;
}