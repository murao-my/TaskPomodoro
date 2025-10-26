# テスト計画書

## テスト方針

### テスト分類

1. **統合テスト**: IQueryable + EF Coreの非同期メソッド（ToListAsync, FirstOrDefaultAsync）を使用するテスト
2. **ユニットテスト**: 単純なCRUD操作（GetByIdAsync, AddAsync, UpdateAsync, DeleteAsync）を使用するテスト

---

## TasksController テスト一覧

| テストケース | 分類 | テストフェーズ | 入力値 | 期待結果 | 説明 |
|-------------|------|-------------|--------|----------|------|
| GetTasks_正常系_全件取得 | 正常系 | **統合テスト** | status=null | 200 OK, 全タスク返却 | パラメータなしで全件取得（ToListAsync使用） |
| GetTasks_正常系_アクティブ取得 | 正常系 | **統合テスト** | status="active" | 200 OK, アクティブタスクのみ | アクティブタスクのみ取得（ToListAsync使用） |
| GetTasks_正常系_アーカイブ取得 | 正常系 | **統合テスト** | status="archived" | 200 OK, アーカイブタスクのみ | アーカイブタスクのみ取得（ToListAsync使用） |
| GetTasks_異常系_無効なstatus | 異常系 | ユニットテスト | status="invalid" | 400 BadRequest | 無効なstatusパラメータ（実行前に失敗） |
| GetTask_正常系_存在するID | 正常系 | ユニットテスト | id=1 | 200 OK, 該当タスク返却 | 存在するタスクIDで取得（GetByIdAsync） |
| GetTask_異常系_存在しないID | 異常系 | ユニットテスト | id=999 | 404 NotFound | 存在しないタスクID |
| GetTask_異常系_負のID | 異常系 | ユニットテスト | id=-1 | 404 NotFound | 負のID（実行前に失敗） |
| GetTask_異常系_ゼロID | 異常系 | ユニットテスト | id=0 | 404 NotFound | ゼロID（実行前に失敗） |
| CreateTask_正常系_最小データ | 正常系 | ユニットテスト | title="A", note=null, estimatedPomos=null | 201 Created | 最小限のデータで作成 |
| CreateTask_正常系_最大データ | 正常系 | ユニットテスト | title=100文字, note=1000文字, estimatedPomos=100 | 201 Created | 最大値のデータで作成 |
| CreateTask_異常系_タイトル空 | 異常系 | ユニットテスト | title="" | 400 BadRequest | 空のタイトル（ModelState検証） |
| CreateTask_異常系_タイトルnull | 異常系 | ユニットテスト | title=null | 400 BadRequest | nullのタイトル（ModelState検証） |
| CreateTask_異常系_タイトル超過 | 異常系 | ユニットテスト | title=101文字 | 400 BadRequest | タイトル文字数超過 |
| CreateTask_異常系_ノート超過 | 異常系 | ユニットテスト | note=1001文字 | 400 BadRequest | ノート文字数超過 |
| CreateTask_異常系_推定ポモ数下限 | 異常系 | ユニットテスト | estimatedPomos=0 | 400 BadRequest | 推定ポモ数が下限未満 |
| CreateTask_異常系_推定ポモ数上限 | 異常系 | ユニットテスト | estimatedPomos=101 | 400 BadRequest | 推定ポモ数が上限超過 |
| UpdateTask_正常系_全項目更新 | 正常系 | ユニットテスト | 存在するID, 全項目有効 | 200 OK | 全項目を更新 |
| UpdateTask_正常系_部分更新 | 正常系 | ユニットテスト | 存在するID, タイトルのみ更新 | 200 OK | 部分的な更新 |
| UpdateTask_異常系_存在しないID | 異常系 | ユニットテスト | id=999 | 404 NotFound | 存在しないIDで更新 |
| UpdateTask_異常系_無効なデータ | 異常系 | ユニットテスト | 存在するID, 無効なデータ | 400 BadRequest | 無効なデータで更新 |
| DeleteTask_正常系_存在するID | 正常系 | ユニットテスト | id=1 | 204 NoContent | 存在するタスクを削除 |
| DeleteTask_異常系_存在しないID | 異常系 | ユニットテスト | id=999 | 404 NotFound | 存在しないタスクを削除 |

---

## SessionsController テスト一覧

| テストケース | 分類 | テストフェーズ | 入力値 | 期待結果 | 説明 |
|-------------|------|-------------|--------|----------|------|
| StartSession_正常系_Focusセッション | 正常系 | **統合テスト** | 有効なTaskId, Kind=Focus, PlannedMinutes=25 | 201 Created | Focusセッション開始（FirstOrDefaultAsync使用） |
| StartSession_正常系_Breakセッション | 正常系 | **統合テスト** | 有効なTaskId, Kind=Break, PlannedMinutes=5 | 201 Created | Breakセッション開始（FirstOrDefaultAsync使用） |
| StartSession_正常系_最小時間 | 正常系 | **統合テスト** | PlannedMinutes=1 | 201 Created | 最小時間でセッション開始 |
| StartSession_正常系_最大時間 | 正常系 | **統合テスト** | PlannedMinutes=120 | 201 Created | 最大時間でセッション開始 |
| StartSession_異常系_存在しないTaskId | 異常系 | ユニットテスト | TaskId=999 | 404 NotFound | 存在しないタスクID |
| StartSession_異常系_無効なKind | 異常系 | ユニットテスト | Kind=999 | 400 BadRequest | 無効なセッション種別（実行前に失敗） |
| StartSession_異常系_時間下限 | 異常系 | ユニットテスト | PlannedMinutes=0 | 400 BadRequest | 時間が下限未満（実行前に失敗） |
| StartSession_異常系_時間上限 | 異常系 | ユニットテスト | PlannedMinutes=121 | 400 BadRequest | 時間が上限超過（実行前に失敗） |
| StartSession_異常系_重複セッション | 異常系 | **統合テスト** | 既存のアクティブセッション | 409 Conflict | 重複するアクティブセッション（FirstOrDefaultAsync使用） |
| CompleteSession_正常系_自動時間計算 | 正常系 | ユニットテスト | 有効なID, EndedAt=null | 200 OK | 自動時間計算で完了 |
| CompleteSession_正常系_手動時間指定 | 正常系 | ユニットテスト | 有効なID, ActualMinutes=30 | 200 OK | 手動時間指定で完了 |
| CompleteSession_異常系_存在しないID | 異常系 | ユニットテスト | id=999 | 404 NotFound | 存在しないセッションID |
| CompleteSession_異常系_既に完了 | 異常系 | ユニットテスト | 完了済みセッション | 409 Conflict | 既に完了済みのセッション |
| CompleteSession_異常系_無効な時間 | 異常系 | ユニットテスト | ActualMinutes=-1 | 400 BadRequest | 無効な時間値（実行前に失敗） |
| GetSession_正常系_存在するID | 正常系 | ユニットテスト | id=1 | 200 OK | 存在するセッションIDで取得 |
| GetSession_異常系_存在しないID | 異常系 | ユニットテスト | id=999 | 404 NotFound | 存在しないセッションID |
| GetSessions_正常系_指定日 | 正常系 | **統合テスト** | date="2024-01-01" | 200 OK | 指定日のセッション取得（ToListAsync使用） |
| GetSessions_正常系_全期間 | 正常系 | **統合テスト** | date=null | 200 OK | 全期間のセッション取得（ToListAsync使用） |
| GetSessions_異常系_無効な日付 | 異常系 | ユニットテスト | date="invalid" | 400 BadRequest | 無効な日付形式 |

---

## SummaryController テスト一覧

| テストケース | 分類 | テストフェーズ | 入力値 | 期待結果 | 説明 |
|-------------|------|-------------|--------|----------|------|
| Get_正常系_有効な期間で集計取得 | 正常系 | **統合テスト** | from="2024-01-01", to="2024-01-03" | 200 OK | 有効な期間で集計取得（複雑な集計ロジック） |
| Get_正常系_単一日の集計取得 | 正常系 | **統合テスト** | from="2024-01-01", to="2024-01-01" | 200 OK | 単一日の集計取得 |
| Get_正常系_セッションなしの期間 | 正常系 | **統合テスト** | from="2024-01-01", to="2024-01-03" | 200 OK, 空の集計 | セッションなしの期間（欠損日補完） |
| Get_正常系_未完了セッションは集計対象外 | 正常系 | **統合テスト** | 完了済みと未完了が混在 | 200 OK | 未完了セッションは集計対象外 |
| Get_正常系_長期間の集計取得 | 正常系 | **統合テスト** | from="2024-01-01", to="2024-01-07" | 200 OK | 1週間の集計取得 |
| Get_異常系_無効な日付形式 | 異常系 | ユニットテスト | from="invalid", to="2024-01-01" | 400 BadRequest, "Invalid date format. Use YYYY-MM-DD." | 無効な日付形式 |
| Get_異常系_fromがtoより後 | 異常系 | ユニットテスト | from="2024-01-03", to="2024-01-01" | 400 BadRequest, "'to' must be greater than or equal to 'from'." | from日付がto日付より後 |

---

## 統計

### 統合テストに移行すべきテスト
- **TasksController**: 3テスト（GetTasks関連）
- **SessionsController**: 7テスト（StartSession関連 + GetSessions関連）
- **SummaryController**: 5テスト（Get関連のすべて）
- **合計**: 15テスト

### ユニットテストのままでもよいテスト
- **TasksController**: 19テスト
- **SessionsController**: 11テスト
- **SummaryController**: 2テスト
- **合計**: 32テスト

### 全体
- **統合テスト**: 15テスト
- **ユニットテスト**: 32テスト
- **合計**: 47テスト

---

## 実行コマンド

```bash
# 統合テストのみ実行
cd /home/muraoo/rootbot/TaskPomodoro/Api.Tests
dotnet test --filter "FullyQualifiedName~Integration" --verbosity normal

# ユニットテストのみ実行
dotnet test --filter "FullyQualifiedName~Unit" --verbosity normal

# 全てのテスト実行
dotnet test --verbosity normal

# カバレッジ取得
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```

