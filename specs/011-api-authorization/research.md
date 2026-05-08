# リサーチ: API認可設定

**フィーチャー**: `011-api-authorization` | **日付**: 2026-05-08

## 調査結果

---

### 1. バックエンド認可ミドルウェア

**決定**: `Microsoft.Identity.Web`（`AddMicrosoftIdentityWebApiAuthentication()`）を使用する

**根拠**:
- ASP.NET Core + Azure Entra の公式推奨ライブラリ。JWT Bearer トークン検証・ロールクレームマッピングをまとめて設定できる
- `.NET 10` 対応は `Microsoft.Identity.Web` v3.x で提供されている
- `AddAuthentication` + `AddJwtBearer` を個別設定する方法より設定量が少なく、誤設定リスクが低い
- `appsettings.json` の `AzureAd` セクション（`TenantId`, `ClientId`, `Audience`）を読み込む構成でコードを最小化できる

**採用しなかった代替案**:
- `Microsoft.AspNetCore.Authentication.JwtBearer`（手動設定）: 設定が冗長かつ誤設定リスクが高いため却下
- 認可チェックなし（現在の状態）: セキュリティ要件を満たさないため却下

**バージョン**: `Microsoft.Identity.Web` 3.x（NuGet 最新安定版）

---

### 2. Azure Entra アプリロールのロールクレームマッピング

**決定**: Azure Entra の「アプリロール（App Roles）」機能を使用し、JWT トークンの `roles` クレームをロール識別子として利用する

**根拠**:
- アクセストークンに `roles` クレームが含まれる（グループクレームより管理コストが低い）
- ASP.NET Core の `[Authorize(Roles = "RoleName")]` 属性が `roles` クレームを自動的に参照する
- `Microsoft.Identity.Web` は `roles` クレームの型を `ClaimTypes.Role` にマッピングするため、標準の `[Authorize(Roles)]` 構文がそのまま使用できる

**ロール定義（Azure Entra アプリ登録で設定）**:
| ロール名 | 表示名 | 対象操作 |
|---|---|---|
| `Items.Read` | アイテム閲覧 | アイテム取得（一覧・詳細・履歴・画像取得） |
| `Items.Create` | アイテム作成・更新 | アイテム作成・更新・画像アップロード |
| `Items.Delete` | アイテム削除 | アイテム削除・画像削除 |
| `User` | 一般ユーザー | カテゴリ・収納場所（部屋・棚）の全操作 |

---

### 3. フロントエンドAPIスコープとトークン取得

**決定**: API 用スコープ（`api://<client-id>/access_as_user` 等）を新たな環境変数 `VITE_AZURE_API_SCOPE` で設定し、`msalService.ts` に `acquireTokenForApi()` 関数を追加する

**根拠**:
- 現在の `VITE_AZURE_SCOPES`（`openid,profile,email`）は ID トークン用であり、アクセストークン（`roles` クレームを含む）を取得するためには API スコープが必要
- API スコープをリクエストすることで Azure Entra がアクセストークンにアプリロール（`roles` クレーム）を付与する
- `acquireTokenSilent` を先に試行し、`InteractionRequiredAuthError` の場合のみポップアップ/リダイレクトにフォールバックする（既存の `msalService.ts` 実装パターンに合わせる）

**実装方針**:
```typescript
// .env.development に追加
VITE_AZURE_API_SCOPE=api://<client-id>/access_as_user

// msalService.ts に追加
async function acquireTokenForApi(): Promise<string>
```

---

### 4. フロントエンドの集中APIクライアント

**決定**: 既存の各サービスファイル（`itemService.ts`, `categoryService.ts` 等）を直接変更するのではなく、共通のAPIフェッチ関数（`apiClient.ts`）を新規作成し、認証ヘッダー付与・エラーハンドリングを集約する

**根拠**:
- 現在のサービスファイルはすべて生の `fetch()` を使用しており、各ファイルを個別修正すると漏れが発生しやすい
- 認証ヘッダー付与・401/403 エラーハンドリングを1箇所に集約することで保守性が向上する
- `AppSnackbar.vue`（既存のトーストコンポーネント）を `apiClient.ts` から呼び出す形にする（または Pinia ストア経由でトースト状態を管理する）

**実装方針**:
```typescript
// src/services/apiClient.ts（新規作成）
// - acquireTokenForApi() を呼び出して Bearer トークンを取得
// - fetch() 呼び出しに Authorization ヘッダーを付与
// - 403 → AppSnackbar.show("アクセス権がありません", true) を呼び出す
// - 401 → acquireTokenSilent を再試行し、失敗時は /login にリダイレクト
// 既存サービスの fetch() を apiClient のラッパー関数に置き換え
```

---

### 5. 既存コードのギャップ分析

| 対象 | 現状 | 必要な変更 |
|---|---|---|
| `Directory.Packages.props` | 認証パッケージなし | `Microsoft.Identity.Web` を追加 |
| `appsettings.json` | `AzureAd` セクションなし | `AzureAd` セクション追加（`TenantId`, `ClientId`, `Audience`） |
| `Program.cs` | 認証/認可ミドルウェアなし | `AddMicrosoftIdentityWebApiAuthentication()` + `UseAuthentication()` + `UseAuthorization()` を追加 |
| `ItemsController.cs` | `[Authorize]` なし | 各アクションに `[Authorize(Roles = "...")]` を追加 |
| `CategoriesController.cs` | `[Authorize]` なし | クラス全体に `[Authorize(Roles = "User")]` を追加 |
| `RoomsController.cs` | `[Authorize]` なし | クラス全体に `[Authorize(Roles = "User")]` を追加 |
| `ShelvesController.cs` | `[Authorize]` なし | クラス全体に `[Authorize(Roles = "User")]` を追加 |
| `ImagesController.cs` | `[Authorize]` なし | 各アクションにロール別の `[Authorize(Roles = "...")]` を追加 |
| `.env.development` | API スコープなし | `VITE_AZURE_API_SCOPE` を追加 |
| `msalService.ts` | `acquireTokenForApi()` なし | 関数を追加 |
| 各サービスファイル | Bearer ヘッダーなし | `apiClient.ts` ラッパーを使用するよう置き換え |

---

### 6. CORS 設定への影響

**決定**: `Program.cs` の CORS ポリシー（`"Frontend"`）は変更不要

**根拠**:
- 現在の CORS ポリシーは `AllowAnyHeader()` を含んでおり、`Authorization` ヘッダーの通過が既に許可されている
- `AllowCredentials()` は Bearer トークン方式では不要

---

### 7. OpenAPI/Swagger への影響

**決定**: Phase 1 スコープでは OpenAPI の認証スキーマ定義は追加しない（実装と別タスクとして扱う）

**根拠**:
- 本フィーチャーの主目的はランタイムの認可設定であり、OpenAPI ドキュメントの整備は優先度が低い
- 必要になった場合は別フィーチャーとして追加できる
