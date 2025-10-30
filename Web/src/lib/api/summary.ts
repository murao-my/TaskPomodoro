/**
 * Summary API クライアント
 * GET /api/summary?from=YYYY-MM-DD&to=YYYY-MM-DD - サマリー取得
 */

import { apiClient } from './client';
import type { SummaryResponseDto } from '../../types/api';

/**
 * サマリー取得
 * @param from - 開始日（YYYY-MM-DD形式）
 * @param to - 終了日（YYYY-MM-DD形式）
 * @returns サマリー情報
 */
export async function getSummary(
    from: string,
    to: string,
): Promise<SummaryResponseDto> {
    return apiClient.get<SummaryResponseDto>(
        `/api/summary?from=${from}&to=${to}`,
    );
}


