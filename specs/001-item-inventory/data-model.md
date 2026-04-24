# Data Model: 個人用物品管理

## エンティティ

### 物品 (Item)

- **説明**: 個人在庫に登録される管理対象。
- **属性**:
  - `id`: string (UUID) - サーバー内部で用いる識別子。
  - `name`: string - 物品名称。空文字不可、在庫内で一意。
  - `quantity`: integer - 保有数。1 以上の整数。
  - `createdAt`: string (date-time) - 初回登録日時。
  - `updatedAt`: string (date-time) - 最終更新日時。

## バリデーションルール

- `name` は必須。前後空白を除去後の長さが 1 以上。
- `name` は大文字小文字を区別しない一意制約を満たす。
- `quantity` は必須で 1 以上の整数。
- `createdAt` と `updatedAt` はサーバーで設定し、クライアントは読取専用。
- 表示時はローカル日時形式へ変換する (保存は ISO 8601)。

## データ構造

### JSON 表現例

```json
{
  "id": "ec95d4e0-2557-4f42-a9d0-d673f0490a4d",
  "name": "歯ブラシ",
  "quantity": 2,
  "createdAt": "2026-04-24T10:30:00Z",
  "updatedAt": "2026-04-24T10:30:00Z"
}
```

## 状態と遷移

- `new` -> `saved`: POST で作成し、`createdAt` と `updatedAt` を同値で設定。
- `saved` -> `saved`: 読み取り操作 (一覧/詳細)。
- `saved` -> `updated`: 将来の更新機能追加時、`updatedAt` のみ更新。

## 関係性

現スコープでは `Item` 単体で完結し、他エンティティとのリレーションは持たない。
