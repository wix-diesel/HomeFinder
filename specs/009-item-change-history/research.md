# リサーチ: アイテム履歴一覧表示

**作成日**: 2026-05-06 | **ブランチ**: `009-item-change-history`

## 既存実装の把握

### 決定: 008 フィーチャーで実装済みのコンポーネントをそのまま活用する

| コンポーネント | パス | 状態 |
|---------------|------|------|
| `ItemHistory` エンティティ | `HomeFinder.Core/Entities/ItemHistory.cs` | 実装済み |
| `ItemHistoryChangeType` 列挙型 | `HomeFinder.Core/Entities/ItemHistoryChangeType.cs` | 実装済み (6種別) |
| `IItemHistoryRepository` | `HomeFinder.Application/Repositories/IItemHistoryRepository.cs` | 実装済み |
| `ItemHistoryRepository` | `HomeFinder.Infrastructure/Repositories/ItemHistoryRepository.cs` | 実装済み |
| `ItemHistoryDto` | `HomeFinder.Application/Contracts/ItemHistoryDto.cs` | 実装済み |
| `ItemService.GetItemHistoryAsync` | `HomeFinder.Application/Services/ItemService.cs` | 実装済み（上限 5 件） |
| `GET /api/items/{id}/history` | `HomeFinder.Api/Controllers/ItemsController.cs` | 実装済み（limit パラメータ、最大 5） |
| `itemHistoryService.ts` | `HomeFinder.UI/src/services/itemHistoryService.ts` | 実装済み（limit=5） |
| DB マイグレーション | `20260506024132_AddItemHistory` | 適用済み |

---

## 課題 1: ページネーション方式の選択

### 決定: オフセットベースページネーション（page + pageSize）を採用する

**根拠**:
- 履歴データはイベントログであり削除されない（挿入のみ）ため、カーソルベースよりオフセットで安定する
- ページ番号を URL パラメータとして保持することでブラウザの戻る/共有が自然に機能する
- 実装コストがカーソルベースより低い

**代替案を退けた理由**:
- カーソルベース: 実装複雑度が高く、本機能のユースケースでは過剰
- 全件返却: 履歴が多い場合にパフォーマンス劣化が生じる

**パラメータ仕様**:
- `page`: 1 以上の整数（デフォルト: 1）
- `pageSize`: 1〜100 の整数（デフォルト: 20）
- `totalCount`: レスポンスに含め、フロントでページ数を計算

---

## 課題 2: 二次ソートキーの決定

### 決定: 同時刻の履歴は `ItemHistory.Id`（GUID）降順で並べる

**根拠**:
- GUID は順序保証がないが、EF Core の `OrderByDescending(x => x.Id)` は SQL SERVER の VARCHAR 比較で一貫した決定論的ソートを提供する
- テストでシードデータの挿入順が再現できる
- spec.md の明確化セッションで合意済み

**代替案を退けた理由**:
- 種別順: ビジネスルールとしての優先度がなく恣意的
- 不定順: テストが再現不能になる

---

## 課題 3: 履歴一覧ページの Vue Router 設計

### 決定: `/items/:itemId/history` を新規ルートとして追加する

**根拠**:
- アイテム詳細 `/items/:id` の子パスとして自然な URL 設計
- `itemId` を route param として受け取り、ページ内で API 呼び出しに使用
- ページネーション状態 (`page`) は URL クエリパラメータ `?page=1` で保持しブラウザ履歴に対応

**ルート定義**:
```ts
{ path: '/items/:itemId/history', component: ItemHistoryPage, name: 'ItemHistory' }
```

---

## 課題 4: 履歴一覧ページの UI コンポーネント設計

### 決定: `design/items_histories.html` のタイムラインレイアウトを Vue コンポーネントで再現する

**根拠**:
- デザイン HTML に Tailwind CSS ベースのタイムラインが定義されており、既存のカラートークンを活用できる
- 既存ページ（`ItemDetailPage.vue`）で使用している `recent-activity-card` クラスの構造を拡張

**アイコンマッピング**:
| 変更種別 | アイコン | 背景色クラス |
|---------|---------|------------|
| `QuantityIncreased` | `add` | `bg-primary-container text-on-primary`（青） |
| `QuantityDecreased` | `remove` | `bg-error text-on-error`（赤） |
| その他 | `info` | `bg-surface-container-highest text-on-surface-variant`（グレー） |

---

## 課題 5: アイテム概要情報の取得方法

### 決定: 既存の `GET /api/items/{id}` エンドポイントを再利用する

**根拠**:
- 履歴一覧ページ上段に表示するアイテム情報（名称・説明・在庫・最終更新日時・画像）は `ItemDto` にすべて含まれている
- 新規エンドポイントを追加する必要がない

---

## 課題 6: エラー状態と再試行の実装方針

### 決定: 取得失敗時はエラーメッセージと再試行ボタンを表示し、自動リトライは行わない

**根拠**:
- spec.md 明確化で合意済み
- ユーザーが意図的に再試行するフローが UX として明確
- 既存の `ItemDetailPage.vue` のエラーハンドリングパターンを踏襲

**実装方針**:
```ts
const historyError = ref(false);
// エラー時: historyError.value = true → テンプレートで再試行ボタン表示
// 再試行ボタン押下: fetchHistories() を再実行
```

---

## 未解決事項

なし。すべての NEEDS CLARIFICATION は本リサーチで解消済み。
