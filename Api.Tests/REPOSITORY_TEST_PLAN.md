# Repository/UnitOfWork テスト計画

## テスト方針

### EfRepository<T> のテスト観点

| テストケース | 分類 | 入力値 | 期待結果 | 説明 |
|-------------|------|--------|----------|------|
| AddAsync_正常系_エンティティ追加 | 正常系 | 有効なエンティティ | Taskが完了 | エンティティが追加される |
| AddAsync_正常系_nullエンティティ | 正常系 | null | ArgumentNullException | nullチェック |
| GetByIdAsync_正常系_存在するID | 正常系 | ID=1 | エンティティ返却 | 存在するエンティティ取得 |
| GetByIdAsync_正常系_存在しないID | 正常系 | ID=999 | null返却 | 存在しない場合null |
| GetByIdAsync_異常系_負のID | 異常系 | ID=-1 | エンティティ返却またはnull | 境界値テスト |
| GetByIdAsync_異常系_ゼロID | 異常系 | ID=0 | エンティティ返却またはnull | 境界値テスト |
| UpdateAsync_正常系_エンティティ更新 | 正常系 | 有効なエンティティ | Taskが完了 | エンティティが更新される |
| UpdateAsync_正常系_nullエンティティ | 正常系 | null | ArgumentNullException | nullチェック |
| DeleteAsync_正常系_エンティティ削除 | 正常系 | 有効なエンティティ | Taskが完了 | エンティティが削除される |
| DeleteAsync_正常系_nullエンティティ | 正常系 | null | ArgumentNullException | nullチェック |
| Query_正常系_IQueryable返却 | 正常系 | (なし) | IQueryable<T>返却 | IQueryableが返却される |

### UnitOfWork のテスト観点

| テストケース | 分類 | 入力値 | 期待結果 | 説明 |
|-------------|------|--------|----------|------|
| SaveChangesAsync_正常系_変更あり | 正常系 | 変更されたエンティティ | 1以上の影響行数 | 変更が保存される |
| SaveChangesAsync_正常系_変更なし | 正常系 | 変更なし | 0 | 影響行数0 |
| Repository_正常系_型別リポジトリ取得 | 正常系 | Repository<Task> | Taskリポジトリ返却 | 正しいリポジトリを取得 |
| Repository_正常系_型別リポジトリ取得 | 正常系 | Repository<Session> | Sessionリポジトリ返却 | 正しいリポジトリを取得 |

---

## 統計

- **EfRepository<T> テスト**: 12件
- **UnitOfWork テスト**: 4件
- **合計**: 16件

---

## 実行コマンド

```bash
# 特定のテスト実行
cd /home/muraoo/rootbot/TaskPomodoro/Api.Tests
dotnet test --filter "FullyQualifiedName~EfRepository" --verbosity normal
dotnet test --filter "FullyQualifiedName~UnitOfWork" --verbosity normal

# 全テスト実行
dotnet test --verbosity normal
```

