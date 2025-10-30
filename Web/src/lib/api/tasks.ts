/**
 * Tasks API クライアント
 * GET /api/tasks - タスク一覧取得
 * POST /api/tasks - タスク作成
 * GET /api/tasks/{id} - タスク取得
 * PATCH /api/tasks/{id} - タスク更新
 * DELETE /api/tasks/{id} - タスク削除
 */

import { apiClient } from './client';
import type {
    TaskResponseDto,
    TaskCreateDto,
    TaskUpdateDto,
} from '../../types/api';

/**
 * タスクステータス（フィルタ用）
 */
export type TaskStatus = 'active' | 'archived' | undefined;

/**
 * タスク一覧取得
 * @param status - フィルタ（active/archived/undefined）
 * @returns タスク配列
 */
export async function getTasks(
    status?: TaskStatus,
): Promise<TaskResponseDto[]> {
    const params = status ? `?status=${status}` : '';
    return apiClient.get<TaskResponseDto[]>(`/api/tasks${params}`);
}

/**
 * タスク作成
 * @param data - タスク作成DTO
 * @returns 作成されたタスク
 */
export async function createTask(
    data: TaskCreateDto,
): Promise<TaskResponseDto> {
    return apiClient.post<TaskResponseDto>('/api/tasks', data);
}

/**
 * タスク取得
 * @param id - タスクID
 * @returns タスク情報
 */
export async function getTask(id: number): Promise<TaskResponseDto> {
    return apiClient.get<TaskResponseDto>(`/api/tasks/${id}`);
}

/**
 * タスク更新
 * @param id - タスクID
 * @param data - タスク更新DTO
 * @returns 更新されたタスク情報
 */
export async function updateTask(
    id: number,
    data: TaskUpdateDto,
): Promise<TaskResponseDto> {
    return apiClient.patch<TaskResponseDto>(`/api/tasks/${id}`, data);
}

/**
 * タスク削除
 * @param id - タスクID
 */
export async function deleteTask(id: number): Promise<void> {
    return apiClient.delete(`/api/tasks/${id}`);
}


