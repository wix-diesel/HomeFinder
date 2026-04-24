# Contract: Item Schema

この契約は、個人用物品管理機能で使用する `Item` オブジェクトのデータ構造を定義します。

## Item スキーマ

- `id` (string): UUID 形式の識別子。
- `name` (string): 物品の名称。空文字不可。ストレージ内で一意。
- `quantity` (integer): 個数。1 以上。
- `createdAt` (string / datetime): 初回登録日時 (サーバー設定)。
- `updatedAt` (string / datetime): 最終更新日時 (サーバー設定)。

## JSON Schema 例

```json
{
  "type": "object",
  "properties": {
    "id": { "type": "string", "format": "uuid" },
    "name": { "type": "string", "minLength": 1 },
    "quantity": { "type": "integer", "minimum": 1 },
    "createdAt": { "type": "string", "format": "date-time" },
    "updatedAt": { "type": "string", "format": "date-time" }
  },
  "required": ["id", "name", "quantity", "createdAt", "updatedAt"],
  "additionalProperties": false
}
```

## 契約ルール

- `name` はアプリ内で一意でなければならない。
- `quantity` は 1 以上の整数でなければならない。
- `createdAt` および `updatedAt` は API では ISO 8601 形式、画面ではローカル日時形式で表示する。