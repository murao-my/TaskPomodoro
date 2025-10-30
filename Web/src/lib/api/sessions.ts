/**
 * Sessions API クライアント
 * POST /api/sessions - セッション開始
 * PATCH /api/sessions/{id}/complete - セッション完了
 * GET /api/sessions?date=YYYY-MM-DD - セッション一覧取得
 */

import { apiClient } from './client';
import type {
    SessionResponseDto,
    SessionCreateDto,
    SessionUpdateDto,
} from '../../types/api';

/**
 * セッション開始
 * @param data - セッション作成DTO
 * @returns 作成されたセッション
 */
export async function startSession(
    data: SessionCreateDto,
): Promise<SessionResponseDto> {
    return apiClient.post<SessionResponseDto>('/api/sessions', data);
}

/**
 * セッション完了
 * @param id - セッションID
 * @param data - セッション更新DTO
 * @returns 完了されたセッション情報
 */
export async function completeSession(
    id: number,
    data: SessionUpdateDto,
): Promise<SessionResponseDto> {
    return apiClient.patch<SessionResponseDto>(
        `/api/sessions/${id}/complete`,
        data,
    );
}

/**
 * セッション取得
 * @param id - セッションID
 * @returns セッション情報
 */
export async function getSession(id: number): Promise<SessionResponseDto> {
    return apiClient.get<SessionResponseDto>(`/api/sessions/${id}`);
}

/**
 * セッション一覧取得
 * @param date - 日付（YYYY-MM-DD形式、省略可）
 * @returns セッション配列
 */
export async function getSessions(
    date?: string,
): Promise<SessionResponseDto[]> {
    const params = date ? `?date=${date}` : '';
    return apiClient.get<SessionResponseDto[]>(`/api/sessions${params}`);
}


