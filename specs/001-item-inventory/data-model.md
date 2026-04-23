# Data Model: 個人用物品管理

## エンティティ

### 物品 (Item)

- **説明**: 個人の在庫として管理するアイテム。
- **属性**:
  - `name`: string - 物品の名称。リポジトリ内で一意であること。
  - `quantity`: integer - 数量。0より大きい正の整数。
  - `createdAt`: string / datetime - 物品が最初に登録された日時。
  - `updatedAt`: string / datetime - 最後に更新された日時。

## バリデーションルール

- `name` は必須で、空文字を許容しない。
- `name` は在庫内で重複しない。
- `quantity` は必須で、1 以上の整数である。
- `createdAt` と `updatedAt` は、実装時に読みやすいローカル日時形式で表示する。

## データ構造

### JSON 表現例

```json
{
  "name": "歯ブラシ",
  "quantity": 2,
  "createdAt": "2026-04-24T10:30:00",
  "updatedAt": "2026-04-24T10:30:00"
}
```

## 状態と遷移

- `new` -> `saved`: 新しい物品を登録すると `createdAt` と `updatedAt` を設定して保存。
- `saved` -> `updated`: 物品を編集すると `updatedAt` を更新。

## 関係性

- この機能は単一エンティティ `Item` で構成され、他のエンティティとの関連は現在のスコープでは不要。
