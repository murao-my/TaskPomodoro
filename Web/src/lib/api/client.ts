/**
 * API クライアント基盤
 * - fetch ラッパー
 * - エラーハンドリング
 * - 共通ヘッダー設定
 */

/**
 * API エラー
 * HTTP ステータスコードとメッセージを含む
 */
export class ApiError extends Error {
    public status: number;

    constructor(status: number, message: string) {
        super(message);
        this.name = 'ApiError';
        this.status = status;
    }
}

/**
 * API クライアント
 * 全ての API 通信の基底クラス
 */
export class ApiClient {
    private baseURL: string;

    constructor(baseURL: string) {
        this.baseURL = baseURL;
    }

    /**
     * リクエスト送信
     * @param endpoint API エンドポイント（例: '/api/tasks'）
     * @param options fetch オプション
     * @returns レスポンスデータ
     */
    async request<T>(endpoint: string, options?: RequestInit): Promise<T> {
        const url = `${this.baseURL}${endpoint}`;

        // 共通ヘッダー
        const headers: HeadersInit = {
            'Content-Type': 'application/json',
            ...options?.headers,
        };

        try {
            const response = await fetch(url, {
                ...options,
                headers,
            });

            // エラーハンドリング（4xx, 5xx）
            if (!response.ok) {
                const errorText = await response.text();
                throw new ApiError(
                    response.status,
                    errorText || `HTTP ${response.status}`,
                );
            }

            // 204 No Content の場合は undefined を返す
            if (response.status === 204) {
                return undefined as T;
            }

            return await response.json();
        } catch (error) {
            // ネットワークエラーやタイムアウトなど
            if (error instanceof ApiError) {
                throw error;
            }

            // その他のエラー
            if (error instanceof TypeError) {
                throw new ApiError(0, 'Network error');
            }

            throw new ApiError(0, `Unexpected error: ${error}`);
        }
    }

    /**
     * GET リクエスト
     */
    async get<T>(endpoint: string): Promise<T> {
        return this.request<T>(endpoint, { method: 'GET' });
    }

    /**
     * POST リクエスト
     */
    async post<T>(endpoint: string, body: unknown): Promise<T> {
        return this.request<T>(endpoint, {
            method: 'POST',
            body: JSON.stringify(body),
        });
    }

    /**
     * PATCH リクエスト
     */
    async patch<T>(endpoint: string, body: unknown): Promise<T> {
        return this.request<T>(endpoint, {
            method: 'PATCH',
            body: JSON.stringify(body),
        });
    }

    /**
     * DELETE リクエスト
     */
    async delete(endpoint: string): Promise<void> {
        return this.request<void>(endpoint, { method: 'DELETE' });
    }
}

/**
 * シングルトンインスタンス
 * 環境変数からベース URL を取得
 */
export const apiClient = new ApiClient(
    import.meta.env.VITE_API_BASE || 'http://localhost:5121',
);


