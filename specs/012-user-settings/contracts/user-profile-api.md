### 変更履歴（重要）

- `POST /api/users/me/profile/avatar` は 2026-05-10 に設計変更され、アップロード後は画像の保存のみ行い `204 No Content` を返すようになりました。バックエンドは画像の保存先パスをクライアントに渡しません。
- クライアントは画像表示・プレビューのために常に `GET /api/users/me/profile/avatar` を使用して最新の画像データ（バイナリ）を取得します。フロントエンドでファイルパスを保持・操作しないでください。
- `PUT /api/users/me/profile` のリクエストボディは `displayName` のみを含むものとします（`avatarImagePath` は送らないでください）。

---

フロントエンドの振る舞い:

- 画像を変更する場合は、まず `POST /api/users/me/profile/avatar` に `multipart/form-data` でファイルを送信する（レスポンスボディは期待しない、`204 No Content`）。
- 画像表示・プレビューは常に `GET /api/users/me/profile/avatar` を `fetch` / `apiClient` 経由で取得し、レスポンスのバイナリを `Blob` として扱ってください（例: `URL.createObjectURL(blob)`）。
- プロフィールの保存は `PUT /api/users/me/profile` に `{"displayName":"..."}` を送るだけです。
# API契約: ユーザープロフィール

**フィーチャー**: `012-user-settings` | **日付**: 2026-05-10  
**バージョン**: v1

## 概要

ユーザー設定画面で表示・更新するプロフィール情報（表示名、アイコン、メール）を提供する。更新対象はログインユーザー本人のみ。

## 認証/認可

- 方式: Bearer Token（Azure Entra）
- 必須: 認証済みユーザー
- 権限制約: `me` エンドポイントのみ公開し、他ユーザー指定更新は不可

---

## エンドポイント

### `GET /api/users/me/profile`

プロフィール取得。レコードが未作成の場合は初期値で自動作成して返却する。

#### Response `200 OK`

```json
{
  "entraObjectId": "00000000-0000-0000-0000-000000000000",
  "email": "user@example.com",
  "displayName": "user@example.com",
  "avatarImagePath": "/images/user-avatar-default.svg"
}
```

#### Error

- `401 Unauthorized`: 認証なし/無効トークン

---

### `POST /api/users/me/profile/avatar`

プロフィール画像アップロード。`multipart/form-data` で画像ファイルを受け取り、サーバー側で保存します。

#### Request

`Content-Type: multipart/form-data`

- フィールド名: `file`
- 対応形式: `image/png`, `image/jpeg`
- サイズ上限: 2MB

#### Response `204 No Content`

- 成功時はボディを返しません。クライアントは保存先パスを受け取りません。

#### Error

| Status | Code | 条件 |
|---|---|---|
| 400 | `INVALID_IMAGE_FORMAT` | PNG/JPG 以外の画像 |
| 400 | `IMAGE_TOO_LARGE` | 2MB 超過 |
| 401 | `UNAUTHORIZED` | 認証なし/トークン無効 |
| 500 | `UPLOAD_FAILED` | 画像保存失敗 |

---

### `PUT /api/users/me/profile`

プロフィール更新（表示名・アイコン参照先）。メールアドレスは更新不可。

#### Request

```json
{
  "displayName": "山田😀",
  "avatarImagePath": "/images/users/8b7a.../avatar.jpg"
}
```

#### バリデーション

- `displayName`: 必須、1〜30文字
- `avatarImagePath`: 必須、`POST /api/users/me/profile/avatar` で取得したパス、または既定値 `/images/user-avatar-default.svg`
- `email`: 受け付けない（契約外）

#### Response `200 OK`

```json
{
  "entraObjectId": "00000000-0000-0000-0000-000000000000",
  "email": "user@example.com",
  "displayName": "山田😀",
  "avatarImagePath": "/images/users/8b7a.../avatar.jpg"
}
```

#### Error

| Status | Code | 条件 |
|---|---|---|
| 400 | `VALIDATION_ERROR` | 入力不備（名前文字数超過、空文字など） |
| 401 | `UNAUTHORIZED` | 認証なし/トークン無効 |
| 403 | `FORBIDDEN` | 本人以外を更新しようとした場合（内部的に検出した場合） |
| 409 | `PROFILE_CONFLICT` | 一意制約競合（通常は発生しないが契約として定義） |

---

## フロントエンド契約

- 画像を変更する場合は、先に `POST /api/users/me/profile/avatar` でアップロードし、返却された `avatarImagePath` を `PUT /api/users/me/profile` に設定して保存する
- 保存成功時: 成功トースト表示、ページ遷移なし
- 保存失敗時: 失敗トースト表示
- バリデーションエラー時: 該当フィールドにエラーメッセージを表示
- 設定画面とヘッダーのプロフィール表示は同一データソース（`GET /api/users/me/profile`）を利用
