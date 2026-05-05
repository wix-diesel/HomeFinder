# 実装計画: アイテム画像アップロード

**ブランチ**: `007-item-image-upload` | **日付**: 2026-05-04 | **仕様**: [spec.md](spec.md)
**入力**: `/specs/007-item-image-upload/spec.md` の機能仕様

## 概要

アイテムの追加・編集ページから1つの画像をアップロードし、Azure Blobs に保存する機能。詳細ページおよび一覧ページで画像を表示（アスペクト比保持）。ユーザーが削除可能。バックエンド API (`/api/items/{itemId}/image`) で認可チェックを実施し、Private Azure Blobs へのアクセスを制御。画像は1000x1000に正規化、キャッシング戦略（max-age=86400）をブラウザ側で実装。

## 技術コンテキスト

**言語/バージョン**: TypeScript (frontend Vue 3), C# / .NET 10 (backend)  
**主要依存関係**: Vue 3 + Vite + TypeScript (frontend), ASP.NET Core Web API + Entity Framework Core (backend)  
**ストレージ**: Azure Blob Storage (本番) / Azurite (ローカル開発)、SQL Server (メタデータ)  
**テスト**: Vitest + Vue Test Utils (frontend), xUnit (backend 契約・統合テスト)  
**対象プラットフォーム**: モダンブラウザ（HTML5 File API サポート） + 既存 ASP.NET Core API ホスト  
**プロジェクト種別**: Web application (Vue.js frontend + ASP.NET Core backend)  
**性能目標**: アップロード・表示 3 秒以内、100 件一覧ページ 2 秒以内レンダリング  
**制約**: 
  - 画像は 1 ファイルのみ、10MB 以下、形式（jpg/bmp/png/webp/svg）、解像度 1000x1000 以下（リサイズ対応）
  - Azure Blobs への接続・認証は既知の状態で、設定方法はスコープ外
  - API-First アーキテクチャ、オニオンアーキテクチャ準拠
  
**規模/スコープ**: 
  - 新規 UI コンポーネント: 画像アップロード、削除ボタン、スナックバー通知
  - 新規 API エンドポイント: 3 件（アップロード、取得、削除）
  - DB スキーマ更新: Item エンティティに ImageId フィールド追加

## 憲章チェック

*ゲート: Phase 0 調査前に必ず合格し、Phase 1 設計後に再確認する。*

### 初期ゲート評価 (Phase 0 前)

| 原則 | 要件 | 状態 | 検証 |
|------|------|------|------|
| I. API-First アーキテクチャ | フロントエンド・バックエンドは Web API で通信 | ✅ 合格 | `/api/items/{itemId}/image` 取得 API を設計 |
| II. UTC 内部・JST 表示 | すべてのタイムスタンプ UTC（API は ISO 8601） | ✅ 合格 | 画像メタデータの アップロード日時は UTC で管理 |
| III. 入力値検証の二重防御 | API + DB レイヤーで検証 | ✅ 合格 | ファイル形式・サイズの API 側検証 + DB 制約で実装予定 |
| IV. テスト駆動開発 | 受け入れシナリオ定義・契約テスト実施 | ✅ 合格 | spec.md に BDD シナリオ + エッジケース定義済み |
| V. 成功基準の測定 | SC-001 ～ SC-007 測定タスクを Phase 2 (tasks.md) に記載 | ✅ 合格（予定） | tasks.md 生成時に検証タスク追加予定 |
| VI. ドキュメント・コード同步 | API 契約・コード同步、ディレクトリ構造統一 | ✅ 合格 | contracts/ に画像 API 契約を記述予定 |
| VII. オニオンアーキテクチャ | Core / Application / Infrastructure / Api 層の依存方向 | ✅ 合格 | Image エンティティは Core、Service は Application に配置予定 |
| VIII. ドキュメント言語 | spec/plan/tasks 日本語、コード内コメント日本語 | ✅ 合格 | 本ドキュメント・以降の contracts/research は日本語で記述 |

### 再確認: Phase 1 後の憲章チェック

| 原則 | 要件 | 状態 | 検証 |
|------|------|------|------|
| I. API-First アーキテクチャ | 3 つの API 契約（アップロード、取得、削除） | ✅ 合格 | contracts/ に 3 ファイル作成、すべて GET/POST/DELETE で定義 |
| II. UTC 内部・JST 表示 | uploadedAtUtc を UTC で管理 | ✅ 合格 | Image エンティティに uploadedAtUtc (DateTime2 UTC) 定義 |
| III. 入力値検証の二重防御 | API + DB 層で検証実装 | ✅ 合格 | API 層で形式・サイズ・解像度検証、DB にも CHECK 制約 |
| IV. テスト駆動開発 | 契約テスト・統合テストシナリオ | ✅ 合格 | quickstart.md に 6 つのテストシナリオ・S1-S6 定義 |
| V. 成功基準の測定 | SC-001 ～ SC-007 の検証タスク | ✅ 合格（予定） | quickstart.md に「成功基準チェックリスト」を記載 |
| VI. ドキュメント・コード同期 | contracts/ と実装の一貫性 | ✅ 合格 | 契約ファイルにコード実装例を含める予定 |
| VII. オニオンアーキテクチャ | Core/Application/Infrastructure/Api 層構成 | ✅ 合格 | プロジェクト構成で層構成とファイルパスを明記 |
| VIII. ドキュメント言語 | 日本語で統一 | ✅ 合格 | すべての Markdown ファイルを日本語で作成 |

**結果**: 🎯 **Phase 1 後の憲章チェック合格** — タスク生成フェーズ(/speckit.tasks) へ進行可能

---

## プロジェクト構成

### ドキュメント (本機能)

```text
specs/007-item-image-upload/
├── plan.md              # 本ファイル (✅ 作成済み)
├── spec.md              # 機能仕様 (✅ 作成済み)
├── research.md          # Phase 0 出力 (🔄 生成中)
├── data-model.md        # Phase 1 出力 (🔄 生成中)
├── quickstart.md        # Phase 1 出力 (🔄 生成中)
├── contracts/           # Phase 1 出力 (🔄 生成中)
│   ├── image-upload-api.md
│   ├── image-retrieval-api.md
│   └── image-deletion-api.md
├── checklists/
│   └── requirements.md   # (✅ 作成済み)
└── tasks.md             # Phase 2 出力 (/speckit.tasks コマンド)
```

### ソースコード (リポジトリルート)

```text
# Web アプリケーション構成（オニオンアーキテクチャ）

src/
├── HomeFinder.Core/              # エンティティ・ドメイン例外（外部依存なし）
│   ├── Entities/
│   │   └── Image.cs              # 新規エンティティ
│   └── Errors/
├── HomeFinder.Application/       # サービス・リポジトリIF・DTO（Core のみに依存）
│   ├── Contracts/
│   │   └── ImageUploadRequest.cs # 新規DTO
│   ├── Repositories/
│   │   └── IImageRepository.cs   # 新規リポジトリIF（既存で拡張）
│   └── Services/
│       └── ImageService.cs       # 新規サービス（Result<T> 返り値）
├── HomeFinder.Infrastructure/    # DbContext・リポジトリ実装（Application に依存）
│   ├── Data/
│   │   └── Migrations/           # DB マイグレーション（ImageId フィールド追加）
│   └── Repositories/
│       └── ImageRepository.cs    # 新規リポジトリ実装
├── HomeFinder.Api/               # コントローラー・起動設定（Application + Infrastructure に依存）
│   ├── Controllers/
│   │   └── ImagesController.cs   # 新規コントローラー
│   ├── Errors/
│   └── Program.cs                # Image Service / Repository 登録
└── tests/
    ├── contract/
    │   └── ImageApiContract.cs   # 契約テスト（新規）
    └── integration/
        └── ImageUploadIntegration.cs # 統合テスト（新規）

frontend/ (HomeFinder.UI/)
├── src/
│   ├── components/
│   │   ├── ImageUploader.vue     # 新規: 画像アップロードコンポーネント
│   │   ├── ImagePreview.vue      # 新規: 画像表示コンポーネント
│   │   └── DeleteImageButton.vue # 新規: 削除ボタン
│   ├── pages/
│   │   ├── ItemDetail.vue        # 修正: 画像表示・削除機能追加
│   │   ├── ItemList.vue          # 修正: 画像プレビュー表示追加
│   │   └── ItemEdit.vue          # 修正: アップロード機能追加
│   ├── services/
│   │   └── imageService.ts       # 新規: API 呼び出しサービス
│   └── composables/
│       └── useImageNotification.ts # 新規: スナックバー通知ロジック
└── tests/
    └── unit/
        ├── ImageUploader.spec.ts
        └── ImagePreview.spec.ts
```

**構成方針**: Web アプリケーション構成（frontend + backend）。バックエンドはオニオンアーキテクチャ準拠。新規エンティティ（Image）をコア層に、サービス・リポジトリをアプリケーション層に配置。コントローラーは API 層で画像の CRUD エンドポイントを提供。フロントエンドは Vue 3 コンポーネント化し、既存の Item ページに統合。

## 複雑性トラッキング

> **憲章チェックに違反がないため、このセクションは不要**

初期ゲート評価で8つの憲章原則すべてに合格。追加レイヤーや非標準構成の必要なし。

---

## Phase 0: リサーチ & 未解決事項の解決

### リサーチ タスク

本フェーズでは、以下のテクノロジー選定と実装パターンを調査・確認します。

#### T-001: Azure Blob Storage (Azurite) での画像リサイズ戦略

**タスク説明**: 1000x1000 の正規化画像を、詳細ページ（600x600px）と一覧ページ（80x80px）でどのように配信・表示するか検証。  
**リサーチ対象**:
- Azurite のメタデータ管理（Content-Type、Cache-Control ヘッダ設定）
- 画像を 1000x1000 で保存した場合の容量・パフォーマンス影響
- クライアント側 CSS （object-fit: contain）での正規化画像の表示方法
- SixLabors.ImageSharp（.NET 画像処理）によるリサイズ実装

**成果物**: research.md に記載

---

#### T-002: DotNext.Result<T> による Azure Blob API のエラー処理

**タスク説明**: Application 層のサービスが Result<T> を返却するパターンを確認。  
**リサーチ対象**:
- DotNext.Result<T> の Error バリアント表現
- Azure Blob SDK の BlobContainerClient メソッドが発生させる例外（例: BlobNotFoundException）
- Error を DTO に変換し、API 層で HTTP ステータスコードを決定するパターン

**成果物**: research.md に記載

---

#### T-003: Vue 3 で File Input + Multipart Form Data の実装

**タスク説明**: Vue 3 コンポーネント内で画像ファイル選択・バリデーション・アップロード、スナックバー通知を実装。  
**リサーチ対象**:
- Vue 3 + Composition API での File Input ハンドリング
- FormData の構築とフェッチ API での送信
- Vuetify / Material Design Vue での Snackbar コンポーネント
- Image メタデータ（幅・高さ）の クライアント側取得

**成果物**: research.md に記載

---

#### T-004: Entity Framework Core での Image エンティティ追加・マイグレーション

**タスク説明**: Image エンティティの設計と EF Core マイグレーション。  
**リサーチ対象**:
- Image エンティティのスキーマ（id, BlobUrl, ItemId, UploadedAt, FileFormat, Size など）
- Item ⟷ Image の One-to-One 関係（Item.ImageId は nullable）
- DB マイグレーション (Add-Migration) の実行フロー
- SQL Server の FileStream または VARBINARY(MAX) vs Azure Blob のメタデータのみ DB 保存

**成果物**: research.md に記載

---

### 調査結果の集約

本フェーズ完了後、research.md に以下を記載します：
- Azure 画像配信の最適サイズ（1000x1000 ベース）
- Result<T> エラーハンドリングパターン
- Vue 3 ファイルアップロード実装ガイドライン
- EF Core マイグレーション設計

**成果物**: [research.md](research.md) を生成

---

## Phase 1: 設計・契約・クイックスタート

### 1-1: データモデル設計

**成果物**: [data-model.md](data-model.md)

#### Image エンティティ

```
Entity: Image
├── id: Guid (PK)
├── itemId: Guid (FK to Item)
├── blobUri: string (Azure Blob URL)
├── fileName: string
├── fileFormat: string (jpg|bmp|png|webp|svg)
├── fileSizeBytes: int
├── originalWidth: int
├── originalHeight: int
├── uploadedAt: DateTime (UTC)
└── deletedAt: DateTime? (論理削除用)
```

#### Item エンティティ（拡張）

```
Entity: Item (修正)
├── [既存フィールド...]
└── imageId: Guid? (FK to Image, Nullable)
```

#### Relationships

```
Item.imageId -> Image.id (One-to-One, Nullable on Item side)
```

---

### 1-2: API 契約設計

**成果物**: contracts/ ディレクトリに 3 つのマークダウンファイルを生成

#### Contract 1: Image Upload API

[contracts/image-upload-api.md](contracts/image-upload-api.md)

```
POST /api/items/{itemId}/image
Content-Type: multipart/form-data

Request:
  - image: File (max 10MB, jpg|bmp|png|webp|svg)

Response 200:
  { "imageId": "uuid", "blobUri": "https://..." }

Response 400:
  { "code": "INVALID_FORMAT", "message": "..." }

Response 403:
  { "code": "UNAUTHORIZED", "message": "..." }

Response 413:
  { "code": "FILE_TOO_LARGE", "message": "..." }
```

#### Contract 2: Image Retrieval API

[contracts/image-retrieval-api.md](contracts/image-retrieval-api.md)

```
GET /api/items/{itemId}/image?size=full|thumb

Response 200:
  Content-Type: image/jpeg (ストリーム配信)
  Cache-Control: max-age=86400

Response 403:
  { "code": "UNAUTHORIZED", "message": "..." }

Response 404:
  { "code": "IMAGE_NOT_FOUND", "message": "..." }
```

#### Contract 3: Image Deletion API

[contracts/image-deletion-api.md](contracts/image-deletion-api.md)

```
DELETE /api/items/{itemId}/image

Response 204:
  (No Content)

Response 403:
  { "code": "UNAUTHORIZED", "message": "..." }

Response 404:
  { "code": "IMAGE_NOT_FOUND", "message": "..." }
```

---

### 1-3: クイックスタート設計

**成果物**: [quickstart.md](quickstart.md)

#### ローカル開発環境セットアップ

1. **Azurite 起動**: Docker Compose の azurite-blob サービス確認
2. **DB マイグレーション**: `dotnet ef database update`
3. **フロントエンド起動**: `npm run dev` (Vite dev server)
4. **バックエンド起動**: `dotnet run --project src/HomeFinder.Api`

#### 最初のテスト実行

- **契約テスト**: xUnit で ImageApiContract テスト
- **フロントエンド ユニット**: Vitest で ImageUploader.vue テスト
- **統合テスト**: POST /api/items/{itemId}/image で実際のアップロードテスト

#### ユーザーストーリー動作確認フロー

S1 (P1): 画像アップロード
  1. ItemEdit.vue を開く
  2. ImageUploader コンポーネントでファイル選択
  3. アップロードボタン → POST /api/items/{id}/image
  4. スナックバー: ✅ 成功（緑）

S2 (P1): 詳細ページ画像表示
  1. ItemDetail.vue を開く
  2. ImagePreview コンポーネント → GET /api/items/{id}/image
  3. 600x600px で中央配置表示

S3 (P2): 一覧ページ画像表示
  1. ItemList.vue → 複数 Item をレンダリング
  2. 各 Item に ImagePreview コンポーネント（80x80px）
  3. 画像キャッシュ有効化（max-age=86400）の確認

S4 (P2): 画像削除
  1. ItemDetail.vue の DeleteImageButton クリック
  2. 確認ダイアログ表示 → 確認
  3. DELETE /api/items/{id}/image
  4. スナックバー: ✅ 削除成功（緑）
  5. プレースホルダー表示に切り替わる

---

### 1-4: Agent Context 更新

`.github/copilot-instructions.md` の `<!-- SPECKIT START -->` ～ `<!-- SPECKIT END -->` セクションを更新

```markdown
<!-- SPECKIT START -->
- 現在の計画: `/specs/007-item-image-upload/plan.md`
<!-- SPECKIT END -->
```

---

## Phase 1 完了チェック

**成果物チェックリスト**:
- ✅ data-model.md 生成
- ✅ contracts/ ディレクトリ：3 つの API 契約ファイル
- ✅ quickstart.md 生成
- ✅ .github/copilot-instructions.md 更新

**結果**: 🎯 Phase 1 完了。Phase 2 (tasks.md 生成) へ進行可能

---

## Phase 2 (予定): タスク生成

次のコマンド `/speckit.tasks` で以下を生成：

- **tasks.md**: spec.md + plan.md の design-model + contracts に基づいた、優先度・依存関係順の実装タスク定義
  - バックエンド実装タスク（Image エンティティ、Service、Repository、Controller）
  - フロントエンド実装タスク（コンポーネント、API サービス、ページ修正）
  - テスト タスク（契約・統合・ユニット）
  - チェックリスト更新タスク



