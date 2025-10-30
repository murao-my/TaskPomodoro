# TaskPomodoro TODO（依存関係のインストールは完了済み想定）

> 設計参照: `taskPomodoro_design.md`

## API（ASP.NET Core / MVC Controllers / EF Core + SQLite）

- [x] `global.json` で .NET SDK バージョンを固定
- [x] `AppDbContext` を作成（`DbSet<Task>`, `DbSet<Session>`）
- [x] `appsettings.json` に接続文字列 `Data Source=app.db;Cache=Shared;` を追加
- [x] `appsettings.Development.json` で開発用設定（例: ログ/詳細エラー）
- [x] DI 登録（`DbContext`/Repository/UoW）
- [x] `Task` エンティティ定義（Title 必須/MaxLength 100、Note 任意、EstimatedPomos 任意、IsArchived、CreatedAt）
- [x] `Session` エンティティ定義（TaskId、Kind、PlannedMinutes、ActualMinutes、StartedAt、EndedAt）
- [x] `Task`-`Session` リレーション設定と外部キー制約
- [ ] `DailySummary` は動的集計のため、エンティティではなく計算結果として実装
- [x] データ注釈/Fluent API で検証属性（Required/Range/MaxLength）
- [x] `IRepository<T>` 抽象（Add/Update/Remove/Find/Query）
- [x] `IUnitOfWork` 抽象（`Task<int> SaveChangesAsync()`）
- [x]`EfRepository<T>` 実装（基本 CRUD + `IQueryable` 返却）
- [x] `UnitOfWork` 実装（`DbContext` 内包）
- [x] 初回マイグレーション作成 `Init`
- [x] `dotnet ef database update` で DB 生成
- [ ] ProblemDetails 応答の確認・整形（.NET 8 既定 + 必要な拡張）
- [ ] 例外フィルタ作成（ドメイン例外→ProblemDetails 変換）
- [ ] アクションフィルタ作成（監査: user/route/elapsed）
- [x] CORS を開発用に許可（web からのアクセス）
- [ ] Rate Limiting の最小構成を Program.cs に設定
- [ ] API バージョニング設定（パッケージ導入済み前提の構成）
- [x] Swagger UI/スキーマの有効化と最小設定（パッケージ導入済み前提）

## DTO / Controllers（I/O 契約）

- [x] Task DTO 定義（Create/Update/Response）
- [x] Session DTO 定義（Create/Complete/Response）
- [x] `TasksController` GET `/api/tasks?status=active|archived`（200: `Task[]`）
- [x] `TasksController` POST `/api/tasks`（201: `Task` / 400）
- [x] `TasksController` PATCH `/api/tasks/{id}`（200 / 404）
- [x] `TasksController` DELETE `/api/tasks/{id}`（204 / 404）
- [x] `SessionsController` POST `/api/sessions`（開始: 201 / 400）
- [x] `SessionsController` PATCH `/api/sessions/{id}/complete`（200 / 404 / 409）
- [x] `SessionsController` GET `/api/sessions?date=YYYY-MM-DD`（200: `Session[]`）
- [x] `SummaryController` GET `/api/summary?from&to`（200: 指定期間集計）
- [x] `DailySummary` 計算ロジック実装（Session から動的集計）
- [x] すべてのアクションに `[ProducesResponseType]`（200/201/400/404/409）を付与


## テスト

- [x] xUnitテストプロジェクト作成（Api.Tests）
- [x] テスト用パッケージ追加（Mvc.Testing, SQLite, Moq, FluentAssertions）
- [x] テストディレクトリ構造作成（Controllers/Data/Integration/Helpers/Fixtures/Mocks/TestData/Utils）
- [x] テスト用 appsettings.json 作成（SQLite 接続・ログ抑制）
- [x] xunit.runner.json 作成（並列実行/出力形式の制御）
- [x] 統合テスト用に SQLite テストデータベース構成を用意
- [x] Controller ユニットテスト（Tasks/Sessions/Summary）
- [x] Repository/UnitOfWork ユニットテスト
- [ ] 検証/例外のテスト（ProblemDetails 変換含む）
- [x] テストデータビルダー実装（TestDataBuilder）
- [x] テストフィクスチャ実装（DatabaseFixture - SQLite対応済み）
- [x] テストディレクトリ構造（Integration/ ディレクトリ追加）

## Web（React + Vite + TypeScript）

### Phase 1: プロジェクトセットアップ（完了済み）
- [x] Vite + React + TypeScript プロジェクト作成
- [x] Tamagui Design System インストール（UI コンポーネントライブラリ）
- [x] 環境変数 `.env` 作成（`VITE_API_BASE=http://localhost:5121`）
- [x] ディレクトリ構造整備（`src/lib/`, `src/components/`, `src/pages/`, `src/hooks/`）

### Phase 2: 基盤実装（最優先 - API接続）
- [x] `lib/api/client.ts` - API クライアント基盤（fetch ラッパ、エラーハンドリング）
- [x] `types/api.ts` - API 型定義（DTO/Enum）
- [x] `lib/api/tasks.ts` - Tasks API クライアント（GET/POST/PATCH/DELETE）
- [x] `lib/api/sessions.ts` - Sessions API クライアント（POST/PATCH/GET）
- [x] `lib/api/summary.ts` - Summary API クライアント（GET）
- [x] `lib/time.ts` - 時間ユーティリティ（ms↔mm:ss、ceil 補正）

### Phase 3: 状態管理（コア機能）
- [ ] `hooks/useTimerState.ts` - タイマー状態管理フック
  - 状態定義（Idle/FocusRunning/FocusPaused/BreakRunning/BreakPaused）
  - 状態遷移ロジック
  - 残り時間カウントダウン
- [ ] `hooks/useSession.ts` - セッション管理フック
  - セッション開始時の暫定保存
  - 完了/スキップ時の確定保存ロジック

### Phase 4: UI コンポーネント
- [ ] `components/TaskList.tsx` - タスク一覧コンポーネント
  - 検索機能
  - タスク選択
  - アーカイブ切替
- [ ] `components/Timer.tsx` - タイマーコンポーネント
  - 残り時間表示
  - 開始/一時停止/再開/スキップボタン
- [ ] `components/SummaryCard.tsx` - 本日の成果カード

### Phase 5: ページ統合
- [ ] `pages/Dashboard.tsx` - ダッシュボードページ
  - 2 カラムレイアウト（TaskList + Timer）
  - レスポンシブ対応（縦優先）
  - 各コンポーネントの統合

## アクセシビリティ / 品質

- [ ] キーボード操作/aria/コントラスト対応（WCAG 2.2 AA）
- [ ] axe による自動チェックの実行
- [ ] EditorConfig/Prettier/ESLint 導入と最小設定

## CI / ドキュメント / リポジトリ

- [ ] GitHub Actions で API ビルド・Web ビルドの最小 CI
- [ ] `.gitignore` に `app.db` とビルド成果物の確認/追記
- [ ] README に起動手順/API 概要/開発コマンドを追記
- [ ] 開発データの初期シード（任意）


