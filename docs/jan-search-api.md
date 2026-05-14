# JANコード検索 API ドキュメント

## 概要
JANコードを指定して商品情報を検索する API です。
外部商品検索 API を利用し、ヒットした商品の先頭1件を返します。

## エンドポイント
- Method: `GET`
- Path: `/api/products/{jan}`

## パラメータ
- `jan` (path): 8桁または13桁の数字

## 成功レスポンス
- Status: `200 OK`
- Body:

```json
{
  "name": "商品名",
  "manufacturer": "メーカー名",
  "price": 1234
}
```

補足:
- `manufacturer` は未取得時に `null`。
- `price` は未取得時に `null`。

## エラーレスポンス
- `400 Bad Request`
  - `jan` が8桁/13桁の数字形式でない場合
  - code: `VALIDATION_ERROR`
- `404 Not Found`
  - 商品が見つからない場合
  - code: `PRODUCT_NOT_FOUND`
- `429 Too Many Requests`
  - 外部 API のレートリミット
  - code: `RATE_LIMITED`
- `500 Internal Server Error`
  - 外部 API 認証失敗、または予期しない内部エラー
  - code: `UPSTREAM_AUTH_FAILED` または `INTERNAL_SERVER_ERROR`
- `503 Service Unavailable`
  - 外部 API タイムアウト
  - code: `UPSTREAM_TIMEOUT`

## 設定
`HomeFinder.Api/appsettings*.json` の `JanSearch:ExternalApi` セクションで設定します。

主な設定キー:
- `BaseUrl`
- `ApiKey`
- `ApplicationId`
- `ApiKeyHeaderName`
- `ApiKeyQueryParameterName`
- `JanQueryParameter`
- `AdditionalQueryParameters`

## 楽天API 連携仕様

### 外部リクエスト URL
実装は以下形式で楽天 Product Search API を呼び出します。

```text
https://openapi.rakuten.co.jp/ichibaproduct/api/Product/Search/20250801?format=json&productCode={JANコード}&applicationId={application id}&accessKey={access key}
```

実際の組み立ては `JanSearch:ExternalApi` の設定値で行います。

### 楽天レスポンスの取り扱い
楽天 API の主なレスポンス構造は以下です。

```json
{
  "Products": [
    {
      "Product": {
        "productCode": "4901301417350",
        "productName": "アタックZERO 洗濯洗剤 本体(380g)",
        "makerName": "花王",
        "salesMinPrice": 616
      }
    }
  ]
}
```

本実装では `Products[0].Product` を先頭商品として読み取り、以下にマッピングします。
- `name` <- `productName`
- `manufacturer` <- `makerName`
- `price` <- `salesMinPrice`（未取得時は `minPrice` などの代替キーを使用）

## 実装位置
- Controller: `src/HomeFinder.Api/Controllers/JanProductsController.cs`
- Service Interface: `src/HomeFinder.Application/Services/IJanProductSearchService.cs`
- Service Implementation: `src/HomeFinder.Infrastructure/Services/JanProductSearchService.cs`
- Validator: `src/HomeFinder.Application/Helper/JanValidator.cs`
