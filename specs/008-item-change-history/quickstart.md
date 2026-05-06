# クイックスタート: アイテム変更履歴

**機能**: 008-item-change-history | **日付**: 2026-05-06

## 前提条件

- 006-item-detail-page の実装が完了していること
- Docker 環境が起動済みであること（`docker compose up -d`）
- 既存 DB に接続できること

## 実装順序

```
[1] ItemHistory エンティティ + ChangeType 列挙型（Core）
    ↓
[2] IItemHistoryRepository + ItemHistoryDto（Application）
    ↓
[3] IItemService に GetItemHistoryAsync を追加、ItemService に履歴記録ロジック追加（Application）
    ↓
[4] ItemHistoryRepository 実装（Infrastructure）
    ↓
[5] ItemDbContext 更新 + EF Core マイグレーション（Infrastructure）
    ↓
[6] ItemsController に GET /api/items/{id}/history を追加（Api）
    ↓
[7] itemHistoryService.ts + ItemDetailPage.vue 更新（Frontend）
```

## テストシナリオ

### S1: アイテム作成時に履歴が記録される

```bash
# アイテムを新規作成
POST /api/items
{
  "name": "テスト商品",
  "quantity": 3
}
# → 201 Created, itemId を取得

# 履歴を確認
GET /api/items/{itemId}/history
# → 200 OK
# → histories[0].changeType == "Created"
# → histories[0].description == "アイテムが作成されました"
```

### S2: 数量更新時に履歴が記録される（増加・減少）

```bash
# 数量を増やして更新
PUT /api/items/{itemId}
{ "name": "テスト商品", "quantity": 10 }
# → 200 OK

GET /api/items/{itemId}/history
# → histories[0].changeType == "QuantityIncreased"
# → histories[0].description == "数量が10個に増加しました"

# 数量を減らして更新
PUT /api/items/{itemId}
{ "name": "テスト商品", "quantity": 2 }
# → 200 OK

GET /api/items/{itemId}/history
# → histories[0].changeType == "QuantityDecreased"
# → histories[0].description == "数量が2個に減少しました"
```

### S3: 複数フィールドを同時更新した場合、フィールド数だけ履歴が記録される

```bash
# 名称と値段を同時更新
PUT /api/items/{itemId}
{ "name": "新名称", "quantity": 2, "price": 1500 }
# → 200 OK

GET /api/items/{itemId}/history
# → histories の最初の2件が同一 OccurredAtUtc
# → histories[0].changeType == "NameUpdated"
# → histories[1].changeType == "PriceUpdated"
```

### S4: 直近5件のみが返される

```bash
# アイテムを6回以上更新する
# ...

GET /api/items/{itemId}/history
# → histories.length == 5（最新5件のみ）
```

### S5: 履歴が存在しない場合は空配列が返される

```bash
# 履歴レコードを持たない itemId でリクエスト（作成直後でも Created が記録されるため、
# 通常このケースは発生しないが、DB に直接履歴を削除した状態でテスト）
GET /api/items/{itemId}/history
# → 200 OK
# → histories == []
```

### S6: 存在しないアイテムでは 404 が返される

```bash
GET /api/items/00000000-0000-0000-0000-000000000000/history
# → 404 Not Found
```

## 成功基準チェックリスト

| 成功基準 | 検証方法 |
|----------|----------|
| SC-001: 変更履歴が 100% の操作で記録される | S1〜S3 のシナリオで POST/PUT 後に履歴レコードが DB に存在することを確認 |
| SC-002: 詳細ページを開くだけで履歴5件が確認できる | ブラウザで ItemDetailPage を開き、Recent Activity セクションに最大5件が表示されることを確認 |
| SC-003: 変更種別を色で即座に識別できる | 各種別のカードに正しい CSS クラス（.created / .positive / .neutral / .other-update）が適用されることを目視確認 |
| SC-004: 複数項目同時更新で各変更が記録される | S3 シナリオで同一 OccurredAtUtc を持つ複数件の履歴が記録されることを確認 |

## ローカル開発環境セットアップ

```bash
# 1. Docker 起動（SQL Server, MinIO）
docker compose up -d

# 2. マイグレーション適用
cd src
dotnet ef database update --project HomeFinder.Infrastructure --startup-project HomeFinder.Api

# 3. バックエンド起動
dotnet run --project HomeFinder.Api

# 4. フロントエンド起動
cd HomeFinder.UI
pnpm install
pnpm dev
```

## 実装メモ

- `ItemService.UpdateItemAsync` では、更新前に旧値を記録してから更新後の値と比較し、変化があったフィールドのみ履歴を記録する
- `OccurredAtUtc` は `DateTime.UtcNow` を一度だけ取得し、同一操作内のすべての履歴レコードに同じ値を設定する（同時更新の識別を可能にするため）
- フロントエンドで `occurredAtUtc` を JST 表示する際は `new Intl.DateTimeFormat('ja-JP', { timeZone: 'Asia/Tokyo', ... })` を使用する

## 実装済みファイル（2026-05-06 時点）

- `src/HomeFinder.Core/Entities/ItemHistory.cs`
- `src/HomeFinder.Core/Entities/ItemHistoryChangeType.cs`
- `src/HomeFinder.Application/Contracts/ItemHistoryDto.cs`
- `src/HomeFinder.Application/Repositories/IItemHistoryRepository.cs`
- `src/HomeFinder.Infrastructure/Repositories/ItemHistoryRepository.cs`
- `src/HomeFinder.Infrastructure/Data/ItemDbContext.cs`
- `src/HomeFinder.Application/Services/IItemService.cs`
- `src/HomeFinder.Application/Services/ItemService.cs`
- `src/HomeFinder.Api/Controllers/ItemsController.cs`
- `src/HomeFinder.Api/Program.cs`
- `src/HomeFinder.UI/src/services/itemHistoryService.ts`
- `src/HomeFinder.UI/src/pages/ItemDetailPage.vue`

## 実行済み検証コマンド

```bash
# バックエンド（契約テスト）
cd src
dotnet test tests/contract/contract.csproj -v minimal

# バックエンド（履歴統合テスト）
dotnet test tests/integration/integration.csproj --filter FullyQualifiedName~ItemHistoryIntegrationTests -v minimal

# フロントエンド（履歴UIテスト）
cd HomeFinder.UI
pnpm test:run src/pages/__tests__/ItemDetailPage.history.spec.ts src/pages/__tests__/ItemDetailPage.historyStyle.spec.ts
```
