# Quickstart: アイテム詳細ページ操作

## 1. 前提

- .NET 10 SDK
- Node.js / pnpm
- 既存 HomeFinder 開発環境が起動可能

## 2. 起動

### Backend

```powershell
cd src
dotnet run --project HomeFinder.Api/HomeFinder.Api.csproj
```

### Frontend

```powershell
cd src/HomeFinder.UI
pnpm install
pnpm dev
```

## 3. 手動確認シナリオ

### シナリオ A: 一覧から詳細へ遷移
1. アイテム一覧ページを開く
2. 任意アイテムを選択する
3. 詳細ページへ遷移し、日本語文言で情報が表示されることを確認

### シナリオ B: 編集ページ遷移
1. 詳細ページ右上3点リーダーを開く
2. 「編集」を押す
3. 編集ページへ遷移することを確認
4. 詳細ページへ戻り、左下「編集」でも同様に遷移することを確認

### シナリオ C: 削除（論理削除）
1. 詳細ページ右上3点リーダーから「削除」を押す
2. 確認ダイアログでキャンセルし、削除されないことを確認
3. 再度削除を実行し、確定する
4. 一覧へ遷移することを確認
5. 削除したアイテムが一覧に表示されないことを確認

### シナリオ D: 対象消失時
1. 詳細表示中のアイテムを別操作で削除済みにする
2. 詳細画面で編集保存または削除実行する
3. 失敗メッセージ表示後に一覧へ遷移することを確認

## 4. 契約確認

- API 契約: contracts/item-detail-api.md
- UI 契約: contracts/item-detail-ui.md

## 5. 既知のスコープ外

- 直近の変更履歴表示
- 履歴ボタンの有効化

## 6. 自動テスト実行コマンド

### バックエンド

```powershell
# 契約テスト
cd src/tests/contract
dotnet test

# 統合テスト
cd src/tests/integration
dotnet test
```

### フロントエンド

```powershell
cd src/HomeFinder.UI
npx vitest run
```

## 7. テスト実行結果サマリー（実装時点）

| テストスイート | 件数 | 結果 |
|--------------|------|------|
| 契約テスト (contract) | 18 件 | ✓ 全 PASS |
| 統合テスト (integration) | 30 件 | ✓ 全 PASS |
| UI ユニットテスト (ItemDetailPage) | 13 件 | ✓ 全 PASS |

## 8. シナリオ検証結果

### SC-001: アイテム詳細を閲覧できる（US1）
- GET `/api/items/{id}` が `canEdit: true`, `canDelete: true` を含む JSON を返すことを統合テストで確認
- `ItemDetailPage` のロードシーケンスをユニットテストで確認（正常表示 / 404 / fetchError の 3 経路）

### SC-002: 存在しないアイテムは 404 を表示する（US1）
- GET `/api/items/{invalid-id}` が 404 を返すことを統合テストで確認
- フロントエンドが 404 時に「見つかりません」メッセージを表示することをユニットテストで確認

### SC-003: 編集ページへ遷移できる（US2）
- `canEdit: true` の場合に編集ボタン/メニューが表示されることをユニットテストで確認
- 編集ボタン押下で `/items/{id}/edit` へ遷移することをユニットテストで確認

### SC-004: 削除確認ダイアログを経て論理削除できる（US3）
- DELETE `/api/items/{id}` が 204 を返すことを統合テストで確認
- 削除成功後にフロントエンドが一覧ページへ遷移することをユニットテストで確認
- 論理削除後に一覧 API からアイテムが除外されることを統合テストで確認

### SC-005: 削除後は一覧に表示されない（US3）
- `DeleteItem_SoftDeleted_ItemDisappearsFromList` 統合テストで確認
- `HasQueryFilter` による EF Core グローバルフィルタで実装済み

### SC-006: 削除失敗時はエラーメッセージを表示する（US3）
- DELETE が 404 返却時はフロントエンドが一覧へ遷移することをユニットテストで確認
- DELETE が一般エラー時はページ内にエラーメッセージを表示することをユニットテストで確認
