# TaskPomodoro TODO（依存関係のインストールは完了済み想定）

> 設計参照: `taskPomodoro_design.md`

## API（ASP.NET Core / MVC Controllers / EF Core + SQLite）

- [x] `global.json` で .NET SDK バージョンを固定
- [x] `AppDbContext` を作成（`DbSet<Task>`, `DbSet<Session>`）
- [ ] `appsettings.json` に接続文字列 `Data Source=app.db;Cache=Shared;` を追加
- [ ] `appsettings.Development.json` で開発用設定（例: ログ/詳細エラー）
- [ ] `Task` エンティティ定義（Title 必須/MaxLength 100、Note 任意、EstimatedPomos 任意、IsArchived、CreatedAt）
- [ ] `Session` エンティティ定義（TaskId、Kind、PlannedMinutes、ActualMinutes、StartedAt、EndedAt）
- [ ] `Task`-`Session` リレーション設定と外部キー制約
- [ ] `DailySummary` は動的集計のため、エンティティではなく計算結果として実装
- [ ] データ注釈/Fluent API で検証属性（Required/Range/MaxLength）
- [ ] `IRepository<T>` 抽象（Add/Update/Remove/Find/Query）
- [ ] `IUnitOfWork` 抽象（`Task<int> SaveChangesAsync()`）
- [ ] `EfRepository<T>` 実装（基本 CRUD + `IQueryable` 返却）
- [ ] `UnitOfWork` 実装（`DbContext` 内包）
- [ ] DI 登録（`DbContext`/Repository/UoW）
- [ ] 初回マイグレーション作成 `Init`
- [ ] `dotnet ef database update` で DB 生成
- [ ] ProblemDetails 応答の確認・整形（.NET 8 既定 + 必要な拡張）
- [ ] 例外フィルタ作成（ドメイン例外→ProblemDetails 変換）
- [ ] アクションフィルタ作成（監査: user/route/elapsed）
- [ ] CORS を開発用に許可（web からのアクセス）
- [ ] Rate Limiting の最小構成を Program.cs に設定
- [ ] API バージョニング設定（パッケージ導入済み前提の構成）
- [ ] Swagger UI/スキーマの有効化と最小設定（パッケージ導入済み前提）

## DTO / Controllers（I/O 契約）

- [ ] Task DTO 定義（Create/Update/Response）
- [ ] Session DTO 定義（Create/Complete/Response）
- [ ] `TasksController` GET `/api/tasks?status=active|archived`（200: `Task[]`）
- [ ] `TasksController` POST `/api/tasks`（201: `Task` / 400）
- [ ] `TasksController` PATCH `/api/tasks/{id}`（200 / 404）
- [ ] `TasksController` DELETE `/api/tasks/{id}`（204 / 404）
- [ ] `SessionsController` POST `/api/sessions`（開始: 201 / 400）
- [ ] `SessionsController` PATCH `/api/sessions/{id}/complete`（200 / 404 / 409）
- [ ] `SessionsController` GET `/api/sessions?date=YYYY-MM-DD`（200: `Session[]`）
- [ ] `SummaryController` GET `/api/summary?from&to`（200: 指定期間集計）
- [ ] `DailySummary` 計算ロジック実装（Session から動的集計）
- [ ] すべてのアクションに `[ProducesResponseType]`（200/201/400/404/409）を付与
- [ ] ModelState 無効時 400（自動検証）の挙動確認

## テスト

- [ ] 統合テスト用に InMemory/別 SQLite へ切替可能な構成を用意
- [ ] Controller ユニットテスト（Tasks/Sessions/Summary）
- [ ] 検証/例外のテスト（ProblemDetails 変換含む）

## Web（React + Vite + TypeScript）

- [ ] `lib/api` 実装（tasks/sessions/summary の fetch ラッパ）
- [ ] `lib/time` 実装（ms↔mm:ss、`ceil` 補正）
- [ ] `components/TaskList`（検索/選択/アーカイブ切替）
- [ ] `components/Timer`（状態遷移/残り時間/開始/一時停止/再開/スキップ）
- [ ] `components/SummaryCard`（本日の成果表示）
- [ ] `pages/Dashboard`（2 カラムレイアウト）
- [ ] タイマー状態管理（Idle/FocusRunning/FocusPaused/BreakRunning/BreakPaused）
- [ ] セッション開始時の暫定保存、完了/スキップ時の確定保存ロジック
- [ ] レスポンシブ最小対応（縦優先、主要ブレークポイント）
- [ ] `VITE_API_BASE` を用いた API ベース URL 利用
- [ ] 開発用のエラーハンドリング・トースト通知（任意）

## アクセシビリティ / 品質

- [ ] キーボード操作/aria/コントラスト対応（WCAG 2.2 AA）
- [ ] axe による自動チェックの実行
- [ ] EditorConfig/Prettier/ESLint 導入と最小設定

## CI / ドキュメント / リポジトリ

- [ ] GitHub Actions で API ビルド・Web ビルドの最小 CI
- [ ] `.gitignore` に `app.db` とビルド成果物の確認/追記
- [ ] README に起動手順/API 概要/開発コマンドを追記
- [ ] 開発データの初期シード（任意）


