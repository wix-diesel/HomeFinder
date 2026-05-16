# 受け入れテスト定義: JANコード検索API

**目的**
実装前に受け入れ基準（契約テスト）を定義し、実装が満たすべき振る舞いを明確にする。

## 前提
- 環境変数または `appsettings.json` に `JanSearch:RapidApi:ApiKey` が設定されていること（統合実行時のみ）。
- 外部APIはモック可能である（テスト環境ではモックエンドポイントを使用）。

## テスト一覧 (Acceptance / Contract Tests)

### AT-01: 有効なJANで商品情報が返る (AC-001)
- 概要: 有効な13桁または8桁のJANを与えると HTTP 200 と `Product` が返る。
- Given: モック/実APIが該当商品を返す状態
- When: `GET /api/products/4901234567890`
- Then: HTTP 200
  - Body: {
    "name": "商品名",
    "manufacturer": "メーカー名",
    "price": 1234.56
  }
- 検証ポイント:
  - `name` が存在する
  - `manufacturer` が存在する
  - `price` は数値または `null`

### AT-02: 不正なJANは HTTP 400 を返す (AC-004)
- Given: 入力が "abc123" のように数字以外を含む
- When: `GET /api/products/abc123`
- Then: HTTP 400
  - Body: { "error": "Invalid JAN format" }

### AT-03: 存在しないJANは HTTP 404 を返す (AC-003)
- Given: モック/実APIが該当商品を返さない状態
- When: `GET /api/products/0000000000000`
- Then: HTTP 404
  - Body: { "error": "Product not found" }

### AT-04: 楽天 RapidAPI の認証エラー -> HTTP 500 (spec のエッジケース)
- Given: 外部APIが認証エラーを返す
- When: `GET /api/products/4901234567890`
- Then: HTTP 500
  - Body: { "error": "Upstream authentication failure" }

### AT-05: レートリミットは透過して 429 を返す
- Given: 外部APIが 429 を返す
- When: `GET /api/products/4901234567890`
- Then: HTTP 429
  - Body: 外部API のレスポンスを透過（または標準化エラー）

## 契約（Contract）チェック項目
- レスポンスの JSON スキーマは `specs/013-jan-search-api/contracts/rapidapi-product-schema.json` に定義すること。
- 必須フィールド: `name`（string）
- 任意フィールド: `manufacturer`（string）, `price`（number|null）

## 実行手順（開発環境・手動）
1. テスト用のモックサーバを起動（例: WireMock, MockServer）。
2. `appsettings.Test.json` を使い、モックのURLを `JanSearch:RapidApi:BaseUrl` に設定する。
3. 手動テスト: curl で確認

```powershell
curl -i http://localhost:5000/api/products/4901234567890
```

4. 自動化: CI における契約テストは、モック起動→API 実行→スキーマ検証の順で実行する。

## 備考
- 受け入れテストは実装前にレビューし、必要なら mock レスポンスのサンプルを `specs/013-jan-search-api/research.md` に追加してください。
