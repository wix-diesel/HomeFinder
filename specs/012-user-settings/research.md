# リサーチ: ユーザー設定画面

**フィーチャー**: `012-user-settings` | **日付**: 2026-05-10

## 調査結果

---

### 1. プロフィール永続化の配置

**Decision**: バックエンドに `UserProfiles` テーブルを追加し、Azure Entra の `oid` を主キー同等の一意識別子として保持する

**Rationale**:
- 現在のコードベースにはユーザープロフィール永続化エンティティが存在しない
- `authStore.ts` で `oid` / `name` / `email` を既に取得しており、`oid` は不変キーとして扱いやすい
- 仕様の「初回ログイン時に名前・アイコンを初期化」を満たすには、サーバー側で upsert が必要

**Alternatives considered**:
- フロントエンド localStorage のみ保存: デバイス間同期できず却下
- Entra プロファイル直接更新: 本機能スコープ外かつ権限制御が重いため却下

---

### 2. API 契約の形

**Decision**: 認証済み本人専用 API として `GET /api/users/me/profile`、`POST /api/users/me/profile/avatar`、`PUT /api/users/me/profile` を追加する

**Rationale**:
- 仕様 FR-015（本人のみ更新可）を URL レベルで明示できる
- `userId` パス指定を避けることで「他ユーザー更新」の誤実装リスクを下げられる
- 既存 API 設計（コントローラー + DTO）に自然に合流できる

**Alternatives considered**:
- `PUT /api/users/{id}`: 認可ミスの危険が高いため却下
- Graph API 経由で取得/更新: 依存増加と運用複雑化のため却下

---

### 3. メールアドレスの扱い

**Decision**: メールアドレスは読み取り専用とし、API 側でも更新不可フィールドとして扱う

**Rationale**:
- 仕様 FR-004 に一致
- Entra 認証情報との不整合を防げる
- サービス層で `email` 変更要求を無視/拒否することで二重防御を実現できる

**Alternatives considered**:
- メール更新 API を許可: IdP 管理項目との責務衝突により却下

---

### 4. アイコン画像保存戦略

**Decision**: 既存の `Images` テーブルは流用せず、ユーザープロフィールに `AvatarImagePath`（既定画像またはアップロード済み相対パス）を保持する

**Rationale**:
- 既存 `Images` は Item 前提（`ItemId` 必須）で、ユーザー用途への流用はスキーマ破壊的変更になる
- 本機能はまずプロフィール表示・更新が主眼であり、最小変更で達成可能
- 既定画像（グレーの人型）の参照管理が単純

**Alternatives considered**:
- `Images` テーブルを polymorphic 化: 既存機能への影響が大きく却下
- 画像を DB バイナリ保存: 運用効率が悪く却下

---

### 5. バリデーション/エラーハンドリング

**Decision**: 表示名は 1〜30 文字（絵文字含む）を API・UI で同一ルール適用し、失敗時はフィールドエラー + トーストで通知する

**Rationale**:
- 仕様 FR-009 / FR-016 と一致
- 憲章 III（入力値検証の二重防御）に準拠
- 既存の `snackbarStore` と `AppSnackbar.vue` を再利用できる

**Alternatives considered**:
- UI のみ検証: API 経由の不正入力を防げず却下
- API のみ検証: UX が悪化するため却下

---

### 6. デフォルトアイコン配置

**Decision**: `src/HomeFinder.UI/public/images/user-avatar-default.svg` を新規追加し、初回プロフィール作成時の `AvatarImagePath` 初期値として使用する

**Rationale**:
- 既存の静的画像配信方式（`public/images`）に整合
- 画面描画時に追加 API 呼び出し不要
- 仕様で要求される「グレーの人型シルエット」を安定提供できる

**Alternatives considered**:
- Material Symbols の `account_circle` 固定利用: 画像アップロード後の表示方式と不整合になりやすく却下

---

### 7. 憲章適合性の確認（Phase 0）

**Decision**: 本機能は API-First・二重防御・オニオンアーキテクチャ・日本語ドキュメント要件を満たす設計で進める

**Rationale**:
- API 主体でプロフィールを保持し、UI は表示/操作に限定
- Application サービスは `DotNext.Result<T>` を返す
- UTC 時刻は `CreatedAtUtc`/`UpdatedAtUtc` を維持

**Alternatives considered**:
- フロントエンド完結実装: 憲章 I/III に抵触するため却下
