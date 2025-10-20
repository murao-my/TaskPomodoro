using TaskPomodoro.Api.Models;

namespace TaskPomodoro.Api.DTOs;

/// <summary>
/// SessionエンティティとDTO間のマッピングを行う静的クラス
/// </summary>
public static class SessionMapper
{
    /// <summary>
    /// SessionエンティティからSessionResponseDtoにマッピング
    /// </summary>
    /// <param name="session">Sessionエンティティ</param>
    /// <returns>SessionResponseDto</returns>
    public static SessionResponseDto ToResponseDto(Session session)
    {
        return new SessionResponseDto
        {
            Id = session.Id,
            TaskId = session.TaskId,
            Kind = session.Kind,
            PlannedMinutes = session.PlannedMinutes,
            ActualMinutes = session.ActualMinutes,
            StartedAt = session.StartedAt,
            EndedAt = session.EndedAt
        };
    }

    /// <summary>
    /// SessionCreateDtoからSessionエンティティにマッピング
    /// </summary>
    /// <param name="createDto">SessionCreateDto</param>
    /// <returns>Sessionエンティティ</returns>
    public static Session FromCreateDto(SessionCreateDto createDto)
    {
        return new Session
        {
            TaskId = createDto.TaskId,
            Kind = createDto.Kind,
            PlannedMinutes = createDto.PlannedMinutes,
            ActualMinutes = null, // 作成時は未設定
            StartedAt = createDto.StartedAt,
            EndedAt = null // 作成時は未完了
        };
    }

    /// <summary>
    /// SessionUpdateDtoからSessionエンティティにマッピング（既存のエンティティを更新）
    /// </summary>
    /// <param name="updateDto">SessionUpdateDto</param>
    /// <param name="existingSession">既存のSessionエンティティ</param>
    /// <returns>更新されたSessionエンティティ</returns>
    public static Session FromUpdateDto(SessionUpdateDto updateDto, Session existingSession)
    {
        if (updateDto.ActualMinutes.HasValue)
        {
            existingSession.ActualMinutes = updateDto.ActualMinutes.Value;
        }

        if (updateDto.EndedAt.HasValue)
        {
            existingSession.EndedAt = updateDto.EndedAt.Value;
        }

        return existingSession;
    }

    /// <summary>
    /// セッション完了時のマッピング（実際の時間を自動計算）
    /// </summary>
    /// <param name="session">完了するSessionエンティティ</param>
    /// <param name="endedAt">終了時刻（nullの場合は現在時刻）</param>
    /// <returns>完了されたSessionエンティティ</returns>
    public static Session CompleteSession(Session session, DateTime? endedAt = null)
    {
        var endTime = endedAt ?? DateTime.UtcNow;
        session.EndedAt = endTime;

        // 実際の時間を自動計算（分単位）
        if (!session.ActualMinutes.HasValue)
        {
            session.ActualMinutes = (int)(endTime - session.StartedAt).TotalMinutes;
        }

        return session;
    }

    /// <summary>
    /// SessionエンティティのコレクションからSessionResponseDtoのコレクションにマッピング
    /// </summary>
    /// <param name="sessions">Sessionエンティティのコレクション</param>
    /// <returns>SessionResponseDtoのコレクション</returns>
    public static IEnumerable<SessionResponseDto> ToResponseDtos(IEnumerable<Session> sessions)
    {
        return sessions.Select(ToResponseDto);
    }
}
