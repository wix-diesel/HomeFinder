# クイックスタート: アイテム履歴一覧表示

**作成日**: 2026-05-06 | **ブランチ**: `009-item-change-history`

## 前提条件

- .NET 10 SDK インストール済み
- Node.js 20+ / pnpm インストール済み
- SQL Server 起動済み（または Docker Compose で起動）
- `008-item-change-history` の DB マイグレーション適用済み

## 1. リポジトリ準備

```bash
git checkout 009-item-change-history
cd c:\repo\HomeFinder
```

## 2. バックエンド起動

```bash
cd src
dotnet run --project HomeFinder.Api
```

API が `http://localhost:5000` で起動する。

## 3. フロントエンド起動

```bash
cd src/HomeFinder.UI
pnpm install
pnpm dev
```

UI が `http://localhost:5173` で起動する。

## 4. 動作確認

### 4-1. 履歴一覧 API を直接確認

```http
GET http://localhost:5000/api/items/{itemId}/history?page=1&pageSize=20
```

期待レスポンス:
```json
{
  "histories": [...],
  "totalCount": N,
  "page": 1,
  "pageSize": 20,
  "totalPages": M
}
```

### 4-2. UI で確認

1. `http://localhost:5173/items` でアイテム一覧を開く
2. 任意のアイテム行をクリックしてアイテム詳細ページへ遷移
3. 右上の「View History」ボタンを押下
4. 履歴一覧ページ（`/items/:itemId/history`）が開き、上段にアイテム概要、下段にタイムラインが表示される
5. 履歴が 20 件超えの場合、ページネーションコントロールが表示される

## 5. テスト実行

### バックエンド

```bash
cd src
# 契約テスト
dotnet test tests/contract/contract.csproj

# 統合テスト
dotnet test tests/integration/integration.csproj
```

### フロントエンド

```bash
cd src/HomeFinder.UI
pnpm test
```

## 6. 実装チェックリスト（開発者向け）

### Backend

- [ ] `IItemHistoryRepository` に `GetPagedByItemIdAsync(itemId, page, pageSize, ct)` 追加
- [ ] `ItemHistoryRepository` 実装: 二次ソート（ThenByDescending(x => x.Id)）を含む
- [ ] `PagedItemHistoryResponse.cs` DTO 作成
- [ ] `ItemService.GetItemHistoryPagedAsync` 実装（Result<PagedItemHistoryResponse>）
- [ ] `ItemsController.GetItemHistory` を page/pageSize 対応に更新
- [ ] 契約テストを page/pageSize レスポンス形式に合わせて更新

### Frontend

- [ ] `src/models/itemHistory.ts` に `PagedItemHistoryResponse` 型追加
- [ ] `itemHistoryService.ts` の `getItemHistory` をページネーション対応に更新
- [ ] `ItemHistoryPage.vue` 新規作成（上段サマリー + タイムライン + ページネーション）
- [ ] `router/index.ts` に `/items/:itemId/history` ルート追加
- [ ] `ItemDetailPage.vue` の「View History」ボタンを活性化しルーティングに接続

## 7. 注意事項

- `ItemDetailPage.vue` の Recent Activity セクション（最新 5 件表示）は既存のまま変更しない
- 新しい History ページは独立したページとして実装し、詳細ページのコンポーネントを再利用する
- 変更ユーザー欄は固定値「未実装」を表示する（`<span>未実装</span>`）
- 日時表示は既存の `formatUtcToJst` ユーティリティを使用する
