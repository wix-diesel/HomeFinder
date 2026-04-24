# Contract: 登録フォーム Payload マッピング

本契約は、登録画面の入力値から API リクエストを生成する際のマッピングルールを定義する。

## 1. 対象 API

- Method: `POST`
- Path: `/api/items`
- Request Content-Type: `application/json`

## 2. 送信対象フィールド

- `name` (string, required)
- `quantity` (integer, required, minimum: 1)

## 3. UI-only フィールド

次の項目は画面表示と入力は行うが、API 送信には含めない。

- `category`
- `priceInput`
- `note`

## 4. 変換ルール

1. フォーム状態を `ItemRegistrationFormState` として保持する。
2. 送信時に `toCreateItemRequest(formState)` を実行する。
3. 返却オブジェクトは `name`, `quantity` のみを含む。
4. UI-only 項目は破棄せず、送信失敗時の再試行に備えてフォーム状態へ保持する。

## 5. 例

### 5.1 フォーム入力状態

```json
{
  "name": "卓上ライト",
  "quantity": 2,
  "category": "家電",
  "priceInput": "3980",
  "note": "寝室用"
}
```

### 5.2 API 送信 payload

```json
{
  "name": "卓上ライト",
  "quantity": 2
}
```

## 6. エラー時要件

- API エラー発生時、フォーム状態は維持する。
- バリデーションエラーは日本語でフィールド単位表示する。
- 再送信時も同マッピング規則を適用する。
