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

---

## リリースノート（v2.0.0予定）

### 📌 概要
バーコード読み込みによる商品情報の自動取得機能を実装した新バージョンです。
既存 API との互換性を保ちながら、カテゴリの自動登録機能を追加しました。

### 🎯 新機能

#### 1. カテゴリ自動登録
- **対象エンドポイント**: `GET /api/items/lookup` (新規追加)
- **動作**: バーコード検索で得られた商品情報から、カテゴリ名を取得し、自動的に登録
- **処理フロー**:
  1. バーコード → 商品情報取得（楽天 API）
  2. 商品名からカテゴリ候補を抽出
  3. 正規化して既存カテゴリの重複確認
  4. 新規なら追加、既存なら再利用

#### 2. エラーハンドリング強化
- `409 Conflict`: カテゴリ同時登録時の UNIQUE 制約違反 → 既存を再取得
- `429 Too Many Requests`: 外部 API レート制限 → 自動リトライ対応
- `503 Service Unavailable`: 外部 API タイムアウト → 詳細エラー応答
- `400 Bad Request`: 不正なリクエスト形式 → 詳細エラー応答

#### 3. 監査ログ追加
- カテゴリ自動登録イベントを `CategoryImportLogger` で記録
- 同時登録による競合・リトライの追跡が可能
- 運用時の問題調査に活用

### ✅ 互換性

**既存エンドポイント: `GET /api/products/{jan}`**
- ✓ 動作変更なし（後方互換性を維持）
- ✓ レスポンス形式は変更なし
- ✓ 既存クライアント対応不要

**新規エンドポイント: `GET /api/items/lookup`**
- カテゴリ自動登録を含むレスポンス
- 既存フローに影響なし

### 🔧 移行ガイド

#### クライアント側対応（オプション）
- 新しい `/api/items/lookup` エンドポイントを利用する場合、返却されるカテゴリ情報をそのまま使用
- 既存の `/api/products/{jan}` を継続使用する場合、対応不要

#### サーバー側デプロイ
1. データベースマイグレーション実行: `dotnet ef database update --project src/HomeFinder.Infrastructure`
2. アプリケーション再起動
3. ロギング設定確認: `appsettings.json` の `Logging` セクションが適切か確認

### 📊 パフォーマンス
- 外部 API タイムアウト: 3秒（リトライ最大 1回）
- カテゴリ登録: 正規化 + DB クエリで平均 < 100ms

### 🐛 既知の制限
- カテゴリ候補の抽出は楽天 API の返却値に依存（抽出率は環境による）
- 複数言語のカテゴリ名は英数字のみを正規化対象

### 📝 参考ドキュメント
- [実装計画](../specs/015-barcode-category-autofill/plan.md)
- [仕様書](../specs/015-barcode-category-autofill/spec.md)
