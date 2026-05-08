# クイックスタート: API認可設定

**フィーチャー**: `011-api-authorization` | **日付**: 2026-05-08

## 前提条件

- `010-azure-entra-login` フィーチャーが実装・動作確認済みであること
- Azure ポータルへのアクセス権限があること（アプリロール設定のため）
- Docker / Docker Compose がインストール済みであること

---

## Step 1: Azure ポータルでアプリロールを設定する

1. **Azure ポータル** → [Microsoft Entra ID] → [アプリの登録] → HomeFinder アプリを開く
2. [アプリのロール] → [アプリのロールを作成] で以下の4つを追加する:

   | 表示名 | 値 | 種類 |
   |---|---|---|
   | アイテム閲覧 | `Items.Read` | Users/Groups |
   | アイテム作成・更新 | `Items.Create` | Users/Groups |
   | アイテム削除 | `Items.Delete` | Users/Groups |
   | 一般ユーザー | `User` | Users/Groups |

3. [エンタープライズ アプリケーション] → HomeFinder アプリ → [ユーザーとグループ] → テストユーザーに適切なロールを割り当てる

---

## Step 2: バックエンドの appsettings.json に AzureAd セクションを追加する

`src/HomeFinder.Api/appsettings.json`:
```json
{
  "AzureAd": {
    "TenantId": "<your-tenant-id>",
    "ClientId": "<your-client-id>",
    "Audience": "api://<your-client-id>"
  }
}
```

開発環境では `src/HomeFinder.Api/appsettings.Development.json` に機密情報を記述する（Git 管理外）。

---

## Step 3: バックエンドパッケージを追加する

```bash
cd src/HomeFinder.Api
dotnet add package Microsoft.Identity.Web
```

または `src/Directory.Packages.props` に追加:
```xml
<PackageVersion Include="Microsoft.Identity.Web" Version="3.*" />
```

---

## Step 4: Program.cs に認証/認可を追加する

```csharp
// builder.Services.AddCors(...) の前に追加
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();
```

```csharp
// app.UseCors("Frontend") の後に追加
app.UseAuthentication();
app.UseAuthorization();
```

---

## Step 5: フロントエンドの環境変数に API スコープを追加する

`src/HomeFinder.UI/.env.development`:
```
VITE_AZURE_API_SCOPE=api://<your-client-id>/access_as_user
```

---

## Step 6: 動作確認

### バックエンドのみ確認

```bash
# トークンなしで API を呼び出す → 401 が返ることを確認
curl -i http://localhost:5000/api/items
# HTTP/1.1 401 Unauthorized

# 有効なトークン（Items.Read ロールあり）で API を呼び出す → 200 が返ることを確認
curl -i -H "Authorization: Bearer <token>" http://localhost:5000/api/items
# HTTP/1.1 200 OK
```

### フロントエンド E2E 確認

1. `Items.Read` ロールのみを持つユーザーでログインする
2. アイテム一覧ページ → 正常表示されること
3. アイテム削除ボタンを押す → 「アクセス権がありません」トーストが 3〜5 秒表示されること

---

## トラブルシューティング

### 401 が返り続ける
- `appsettings.json` の `TenantId` / `ClientId` / `Audience` が正しいか確認する
- Azure ポータルで API スコープ（`access_as_user`）が公開されているか確認する

### 403 が返る（ロールありユーザーで）
- Azure ポータルの [エンタープライズ アプリケーション] でユーザーにロールが割り当てられているか確認する
- トークンの `roles` クレームを [jwt.ms](https://jwt.ms) でデコードして確認する

### フロントエンドで 401 ループが発生する
- `VITE_AZURE_API_SCOPE` が正しいか確認する
- Azure ポータルで API スコープが公開済み状態（Enabled）か確認する
