# タスク＋ポモドーロ（React＋ASP.NET Core **MVC Controllers**＋SQLite）— 設計方針

> 本設計は **MVC Controllers（ASP.NET Core Web API）** を前提とし、**永続化は SQLite** を採用します。将来的な置換（PostgreSQL／SQL Server／In‑Memory 等）に備え、**Repository＋Unit of Work 抽象**で分離します。

---

## 1) ゴールとスコープ（MVP）
**ゴール**  
- タスクを登録し、選んだタスクで **25分作業 + 5分休憩** のポモドーロを回す。
- 1日の完了ポモ数と累計時間を把握し、達成感を得る。

**MVP機能**  
- タスク CRUD（タイトル／任意メモ／見積りポモ数）
- タイマー（作業25分／休憩5分、スキップ・一時停止・再開）
- セッション記録（開始時刻・種類〔Focus/Break〕・紐づくタスク・所要分）
- 今日のサマリー（完了ポモ数、累計時間）
- **SQLite 永続化**（将来の DB 置き換えに備えた抽象化）

**除外（MVP外）**  
- 認証／チーム共有／詳細統計／Web Push 通知／長期レポート

---

## 2) 全体アーキテクチャ
```
[React(UI)] ── HTTP/JSON ── [ASP.NET Core Web API (MVC Controllers)] ── [EF Core] ── [SQLite]
                                                     │
                                                     └── Repository & Unit of Work（抽象化により DB 置換を許容）
```
- React は **クライアント主導タイマー**（秒単位 UI）。
- API は **Controllers + Actions** で、**タスク／セッション／集計**を分離。
- データ層は **EF Core** を使用。`IRepository<T>` と `IUnitOfWork` で抽象化し、SQLite→他 RDBMS の差替えを容易化。
- エラー応答は `[ApiController]` による **自動モデル検証** と **ProblemDetails** を採用。

---

## 3) ドメインモデル（最小）
**Task**
- `Id`, `Title`, `Note?`, `EstimatedPomos? (int)`, `IsArchived (bool)`, `CreatedAt`

**Session**
- `Id`, `TaskId`, `Kind (Focus|Break)`, `PlannedMinutes`, `ActualMinutes`, `StartedAt`, `EndedAt?`

**DailySummary（派生）**
- `Date`, `FocusPomos`, `FocusMinutes`, `BreakMinutes`（期間指定で動的集計）

---

## 4) 主要ユースケースと画面
**A. タスク一覧**：左ペイン一覧／右ペイン詳細＋開始、ヘッダに本日の🍅数。

**B. タイマー**：`Focus 25:00`→`Break 5:00` 自動遷移。開始／一時停止／再開／スキップ。終了で Session 確定保存。

**C. サマリー**：今日の合計フォーカス時間、完了ポモ数、直近5セッション。

---

## 5) タイマー状態遷移（クライアント主導）
```
Idle
 └─[Start Focus]→ FocusRunning
       ├─[Pause]→ FocusPaused
       ├─[Complete or Skip]→ BreakRunning
       └─[Cancel]→ Idle
BreakRunning
       ├─[Pause]→ BreakPaused
       ├─[Complete or Skip]→ Idle（次のポモ開始はユーザー選択）
       └─[Cancel]→ Idle
```
**保存ポイント**：開始で暫定 Session 作成、完了/スキップで `ActualMinutes` と `EndedAt` を確定保存。

---

## 6) API 設計（MVC Controllers）
### 6.1 コントローラ構成
```
Controllers/
  TasksController.cs       [ApiController][Route("api/tasks")]
  SessionsController.cs    [ApiController][Route("api/sessions")]
  SummaryController.cs     [ApiController][Route("api/summary")]
```

### 6.2 エンドポイント
- **TasksController**
  - `GET /api/tasks?status=active|archived` → 200: `Task[]`
  - `POST /api/tasks`（Body: `Title`, `Note?`, `EstimatedPomos?`） → 201: `Task` / 400（検証）
  - `PATCH /api/tasks/{id}`（任意更新） → 200: `Task` / 404
  - `DELETE /api/tasks/{id}` → 204 / 404

- **SessionsController**
  - `POST /api/sessions`（開始） Body: `TaskId`, `Kind`, `PlannedMinutes`, `StartedAt` → 201: `Session` / 400
  - `PATCH /api/sessions/{id}/complete`（終了確定） Body: `ActualMinutes`, `EndedAt` → 200: `Session` / 404 / 409
  - `GET /api/sessions?date=YYYY-MM-DD` → 200: `Session[]`

- **SummaryController**
  - `GET /api/summary?from=YYYY-MM-DD&to=YYYY-MM-DD` → 200: `{ focusPomos, focusMinutes, breakMinutes }`

**I/O 方針**：`ActionResult<T>` を採用し、`[ProducesResponseType]` で 200/201/400/404/409 を明記。Validation は DataAnnotations（`[Required]`, `[Range]`, `[MaxLength]`）および必要に応じ FluentValidation を併用。

---

## 7) データアクセス設計（SQLite 前提・置換容易）
### 7.1 抽象
- `IRepository<T>`：`Add/Update/Remove/Find/Query` を定義
- `IUnitOfWork`：`Task<int> SaveChangesAsync()` を提供

### 7.2 具象（EF Core + SQLite）
- `AppDbContext`（`DbSet<Task>`, `DbSet<Session>`）
- `EfRepository<T>`（`IRepository<T>` 実装）
- `UnitOfWork`（`IUnitOfWork` 実装、`DbContext` を内包）
- **接続**：`Data Source=app.db;Cache=Shared;`（開発用）。
- **マイグレーション**：`dotnet ef migrations add Init` → `dotnet ef database update`

### 7.3 置換戦略
- PostgreSQL/SQL Server へ移行：`AppDbContext` のプロバイダ切替、接続文字列変更、Migration 再作成。
- In‑Memory／ファイルDB など：`IRepository/IUnitOfWork` の別実装を DI で差替え。

---

## 8) 横断的関心事
- **例外フィルタ**：ドメイン例外→ProblemDetails へ集約（`type`, `title`, `detail`）。
- **アクションフィルタ**：監査ログ（`user`, `route`, `elapsed`）。
- **CORS/RateLimiting**：Program.cs で登録。必要に応じ ETag/Cache-Control を導入。

---

## 9) フロント構成（React）
- `components/TaskList`（選択・検索・アーカイブ切替）
- `components/Timer`（状態遷移、残り時間、操作ボタン）
- `components/SummaryCard`（本日の成果）
- `pages/Dashboard`（2 カラムレイアウト）
- `lib/api`（`tasks`, `sessions`, `summary`）、`lib/time`（計時・フォーマット）

※ API I/O は上記エンドポイントに準拠。

---

## 10) 計時と整合
- 計測は **クライアント基準**。終了時に `ActualMinutes = ceil((Now - StartedAt)/60s)` で補正。
- サーバは受領した `StartedAt/EndedAt` を「事実」として保存。

---

## 11) バリデーション（最小）
- Task：`Title` 必須（最大100文字）
- Session：`PlannedMinutes` は Focus=25, Break=5（固定）。
- `ActualMinutes`：0 < n ≤ planned + 3（許容バッファ）。

---

## 12) テスト観点
- **Controller ユニットテスト**：`DefaultHttpContext`／DI モックでアクション単体検証。
- **検証/例外テスト**：モデル無効時の自動 400（ProblemDetails）・例外フィルタ変換。
- **統合テスト**：`WebApplicationFactory` でエンドツーエンド（SQLite をテスト専用 DB に切替）。

---

## 13) 実装順と時間配分（2h想定）
1. **EF Core + SQLite 配線**（DbContext／接続文字列／Migrations）… 25分  
2. **Controllers と DTO**（Tasks/Sessions/Summary、属性ルーティング、検証）… 35分  
3. **Repository/UoW 実装と DI** … 15分  
4. **React レイアウト＆I/O 接続**（一覧・タイマー・サマリー）… 30分  
5. **スモークテスト＆ProblemDetails 整形** … 15分

---

## 14) 拡張ポイント
- DB 差替え：`AppDbContext` のプロバイダ切替／`IRepository` 実装差替え。
- 設定化：`focus/break/longBreak/interval` をユーザー設定に。
- 通知：ページ非アクティブ時のデスクトップ通知（Notification API）。
- 分析：週間ヒートマップ、タスク別消費時間ランキング。

---

### まとめ
- **MVC Controllers＋EF Core（SQLite）**で堅実に構成し、**Repository/UoW** で DB 置換可能性を担保。  
- フロントはクライアント主導タイマーで体験を最優先。  
- 2時間で MVP を完成させ、将来の拡張（DB・バージョン・横断機能）に滑らかに対応。



---

## 付録A：初期セットアップ & 依存関係インストール（コマンド集）
> OSを問わず実行可能な `.NET 8 / Node 18+` 前提。PowerShell/BashどちらでもOK。

### 1) リポジトリ雛形
```bash
mkdir task-pomodoro && cd task-pomodoro
```

### 2) バックエンド（ASP.NET Core Web API：MVC Controllers）
```bash
# プロジェクト作成（Controllers テンプレート）
dotnet new webapi -o api
cd api

# 必須: EF Core + SQLite + デザイン時ツール
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Design

# API ドキュメント（Swagger / OpenAPI）
dotnet add package Swashbuckle.AspNetCore

# 入力検証（必要に応じて）
#dotnet add package FluentValidation.AspNetCore
# DI拡張（FluentValidation を DI 登録する場合）
#dotnet add package FluentValidation.DependencyInjectionExtensions

# API バージョニング（任意：将来の拡張に備える）
dotnet add package Asp.Versioning.Mvc
# Swagger と連携する場合（任意）
dotnet add package Asp.Versioning.Mvc.ApiExplorer

# （任意）レート制限・他は .NET 8 標準のため追加パッケージ不要

# EF CLI（グローバル／またはローカルツールのどちらかを選択）
# グローバル
 dotnet tool install --global dotnet-ef
# もしくはローカル
# dotnet new tool-manifest
# dotnet tool install dotnet-ef

# 初回マイグレーション（DbContext 実装後に実行）
# dotnet ef migrations add Init
# DB 生成
# dotnet ef database update

# 実行
# dotnet run
```

> **メモ**
> - 接続文字列例：`Data Source=app.db;Cache=Shared;` を `appsettings.json` に設定。
> - テスト用 DB は `app.test.db` 等に切替え、`DbContextOptions` を DI で出し分け。

### 3) フロントエンド（React + Vite + TypeScript）
```bash
cd ..
# Vite テンプレートで作成
npm create vite@latest web -- --template react-ts
cd web
npm i

#（任意）ルーティング・型検証など
# npm i react-router-dom
# npm i zod

# 開発起動
npm run dev
```

> **環境変数（任意）**
> - `web/.env` に `VITE_API_BASE=http://localhost:5000` のように API ベースURLを設定可能。
> - 逆に同一オリジンで提供する場合は不要（リバースプロキシや同一ホストで配信）。

### 4) ディレクトリ
```
/task-pomodoro
  /api
    Controllers/
    Models/
    Data/        # AppDbContext, Migrations
    Repositories/ # IRepository<T>, EfRepository<T>, UnitOfWork
    Filters/     # 例外/監査（任意）
  /web
    src/components/
    src/pages/
    src/lib/
```

### 5) よく使うコマンド早見表
```bash
# バックエンド
cd api
# マイグレーション生成（変更反映）
dotnet ef migrations add <Name>
# DB更新
dotnet ef database update
# 実行
dotnet run

# フロントエンド
cd ../web
npm run dev   # 開発
npm run build # 本番ビルド
```

## 付録B：ディレクトリ構成を作るコマンド（Bash / PowerShell）
> ルート：`task-pomodoro/`（付録Aの続き）。既存の `api` / `web` を前提に**不足ディレクトリを一気に作成**します。

### Bash（macOS / Linux / Git Bash）
```bash
cd task-pomodoro

# API 側
mkdir -p api/{Controllers,Models,Data,Migrations,Repositories,Filters}
# 空ディレクトリをgit管理したい場合のプレースホルダー
#: > api/Data/.gitkeep
#: > api/Migrations/.gitkeep
#: > api/Repositories/.gitkeep
#: > api/Filters/.gitkeep
#: > api/Models/.gitkeep

# Web 側
mkdir -p web/src/{components,pages,lib}
#: > web/src/components/.gitkeep
#: > web/src/pages/.gitkeep
#: > web/src/lib/.gitkeep