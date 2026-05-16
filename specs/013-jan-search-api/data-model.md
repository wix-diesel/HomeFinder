# データモデル: JANコード検索 API

## Product
JAN検索で返却するレスポンスモデル。

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| name | string | Yes | 商品名 |
| manufacturer | string \| null | No | メーカー名。取得不可の場合は null |
| price | number \| null | No | 価格。取得不可の場合は null |

## JanSearchRequest
API パスパラメータで受ける検索キー。

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| jan | string | Yes | 8桁または13桁の数字 |

## エラー応答
共通エラー形式 `ApiError` を使用。

| Field | Type | Required | Notes |
|-------|------|----------|-------|
| code | string | Yes | エラーコード |
| message | string | Yes | エラーメッセージ |
| details | array | No | バリデーション詳細 |
