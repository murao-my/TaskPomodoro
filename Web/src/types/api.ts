/**
 * API 型定義
 */

/**
 * セッション種類（enum 代替：erasableSyntaxOnly 対応）
 */
export const SessionKind = {
    Focus: 0,
    Break: 1,
} as const;
export type SessionKind = (typeof SessionKind)[keyof typeof SessionKind];

/**
 * タスク応答DTO
 */
export interface TaskResponseDto {
    id: number;
    title: string;
    note?: string | null;
    estimatedPomos?: number | null;
    isArchived: boolean;
    createdAt: string;
}

/**
 * タスク作成DTO
 */
export interface TaskCreateDto {
    title: string;
    note?: string | null;
    estimatedPomos?: number | null;
}

/**
 * タスク更新DTO
 */
export interface TaskUpdateDto {
    title: string;
    note?: string | null;
    estimatedPomos?: number | null;
    isArchived: boolean;
}

/**
 * セッション応答DTO
 */
export interface SessionResponseDto {
    id: number;
    taskId: number;
    kind: SessionKind;
    plannedMinutes: number;
    actualMinutes?: number | null;
    startedAt: string;
    endedAt?: string | null;
    status: string;
    durationMinutes?: number | null;
}

/**
 * セッション作成DTO
 */
export interface SessionCreateDto {
    taskId: number;
    kind: SessionKind;
    plannedMinutes: number;
    startedAt: string;
}

/**
 * セッション更新DTO
 */
export interface SessionUpdateDto {
    actualMinutes?: number | null;
    endedAt?: string | null;
}

/**
 * サマリー応答DTO
 */
export interface SummaryResponseDto {
    from: string;
    to: string;
    days: DailySummaryDto[];
}

/**
 * 日次サマリーDTO
 */
export interface DailySummaryDto {
    date: string;
    focusMinutes: number;
    breakMinutes: number;
    totalSessions: number;
    completedSessions: number;
}


