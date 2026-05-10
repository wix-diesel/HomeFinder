# データモデル: ユーザー設定画面

**フィーチャー**: `012-user-settings` | **日付**: 2026-05-10

## 概要

本フィーチャーでは、ユーザープロフィールの永続化のために `UserProfile` エンティティを新規追加する。認証情報は Azure Entra を正とし、アプリ側ではプロフィール表示名とアイコン参照を管理する。

---

## エンティティ

### UserProfile

| フィールド | 型 | 必須 | 制約 | 説明 |
|---|---|---|---|---|
| `Id` | `Guid` | Yes | PK | 内部識別子 |
| `EntraObjectId` | `string` | Yes | 最大 100 文字・UNIQUE | Azure Entra `oid` |
| `Email` | `string` | Yes | 最大 320 文字 | 読み取り専用表示用メール |
| `DisplayName` | `string` | Yes | 1〜30 文字 | ユーザー表示名（絵文字可） |
| `AvatarImagePath` | `string` | Yes | 最大 512 文字 | 既定画像またはアップロード画像の相対パス |
| `CreatedAtUtc` | `DateTime` | Yes | UTC | 作成日時 |
| `UpdatedAtUtc` | `DateTime` | Yes | UTC | 更新日時 |

---

## 状態遷移

### 初回ログイン時

1. `GET /api/users/me/profile` 呼び出し
2. `EntraObjectId` に一致する `UserProfile` が存在しない場合は新規作成
3. 初期値:
   - `DisplayName` = `Email`
   - `AvatarImagePath` = `/images/user-avatar-default.svg`

### 更新時

1. `PUT /api/users/me/profile` で `DisplayName` と `AvatarImagePath` を受領
2. バリデーションを通過した場合のみ更新
3. `UpdatedAtUtc` を UTC 現在時刻で更新

---

## バリデーション規則

- `DisplayName`
  - 必須
  - 1〜30 文字
  - 前後空白はトリムして判定
- `Email`
  - API 更新対象外（読み取り専用）
- `AvatarImagePath`
  - 必須
  - 許可拡張子: `.png`, `.jpg`, `.jpeg`, `.svg`
  - 最大入力長: 512

---

## DB 制約（憲章 III: 二重防御）

- UNIQUE: `EntraObjectId`
- CHECK: `LEN(DisplayName) BETWEEN 1 AND 30`
- NOT NULL: `EntraObjectId`, `Email`, `DisplayName`, `AvatarImagePath`, `CreatedAtUtc`, `UpdatedAtUtc`

---

## DTO モデル（Application 層）

### UserProfileDto

- `string EntraObjectId`
- `string Email`
- `string DisplayName`
- `string AvatarImagePath`

### UpdateUserProfileRequest

- `string DisplayName`
- `string AvatarImagePath`

`Email` は更新要求に含めない。
