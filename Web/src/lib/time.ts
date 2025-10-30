/**
 * 時間ユーティリティ
 * - ミリ秒 ↔ 分:秒 の変換
 * - 切り上げ補正
 */

/**
 * ミリ秒を 分:秒 形式の文字列に変換
 * @param milliseconds - ミリ秒
 * @returns 分:秒 形式の文字列（例: "25:00"）
 */
export function millisecondsToMinutesSeconds(
    milliseconds: number,
): string {
    const totalSeconds = Math.floor(milliseconds / 1000);
    const minutes = Math.floor(totalSeconds / 60);
    const seconds = totalSeconds % 60;

    return `${String(minutes).padStart(2, '0')}:${String(seconds).padStart(2, '0')}`;
}

/**
 * 分:秒 形式の文字列をミリ秒に変換
 * @param timeString - 分:秒 形式の文字列（例: "25:00"）
 * @returns ミリ秒
 */
export function minutesSecondsToMilliseconds(timeString: string): number {
    const [minutes, seconds] = timeString.split(':').map(Number);
    return (minutes * 60 + seconds) * 1000;
}

/**
 * 分をミリ秒に変換
 * @param minutes - 分数
 * @returns ミリ秒
 */
export function minutesToMilliseconds(minutes: number): number {
    return minutes * 60 * 1000;
}

/**
 * ミリ秒を分に変換（切り上げ）
 * @param milliseconds - ミリ秒
 * @returns 分数（切り上げ）
 */
export function millisecondsToMinutesCeil(milliseconds: number): number {
    return Math.ceil(milliseconds / (60 * 1000));
}

/**
 * 残り時間の計算（残り分:秒）
 * @param totalMilliseconds - 総時間（ミリ秒）
 * @param elapsedMilliseconds - 経過時間（ミリ秒）
 * @returns 残り時間の文字列（分:秒形式）
 */
export function getRemainingTime(
    totalMilliseconds: number,
    elapsedMilliseconds: number,
): string {
    const remaining = Math.max(0, totalMilliseconds - elapsedMilliseconds);
    return millisecondsToMinutesSeconds(remaining);
}

/**
 * 進捗率の計算（0-1）
 * @param totalMilliseconds - 総時間（ミリ秒）
 * @param elapsedMilliseconds - 経過時間（ミリ秒）
 * @returns 進捗率（0-1）
 */
export function getProgress(
    totalMilliseconds: number,
    elapsedMilliseconds: number,
): number {
    if (totalMilliseconds === 0) return 0;
    return Math.min(1, elapsedMilliseconds / totalMilliseconds);
}


