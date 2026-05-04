# 実装タスク: アイテム画像アップロード

**フィーチャー**: 007-item-image-upload  
**ブランチ**: `007-item-image-upload`  
**作成日**: 2026-05-04  
**仕様書**: [spec.md](spec.md) | [plan.md](plan.md) | [data-model.md](data-model.md)  
**API 契約**: [contracts/](contracts/)

---

## ユーザーストーリーと優先度

### ユーザーストーリー 1 (US1): 画像アップロード [P1]
アイテム追加・編集ページから、1つの画像をアップロードし、Azure Blobsに保存。  
**独立テスト基準**: 新規アイテムを作成し、画像をアップロードして登録完了。

### ユーザーストーリー 2 (US2): 詳細ページでの画像表示 [P1]
アイテム詳細ページで登録された画像を表示（600x600px）。画像なしの場合はプレースホルダー。  
**独立テスト基準**: 画像登録済み・未登録の両アイテムを詳細表示で確認。

### ユーザーストーリー 3 (US3): 一覧ページでの画像表示 [P2]
アイテム一覧ページで各アイテムの代表画像を80x80pxのプレビューで表示。  
**独立テスト基準**: 複数アイテムの一覧で、画像の有無が正しく表示される。

### ユーザーストーリー 4 (US4): 画像削除 [P2]
アイテム詳細ページから「画像削除」ボタンで削除確認後、Azure Blobsから削除。  
**独立テスト基準**: 画像削除後、プレースホルダーが表示される。

---

## タスク一覧

### フェーズ 1: セットアップ (初期化・依存関係)

プロジェクト全体の初期化、NuGet パッケージ導入、npm 依存関係、Docker/Azurite 環境設定。

- [ ] T1-001 [P] セットアップ: バックエンド SixLabors.ImageSharp NuGet パッケージをインストール（Infrastructure 層のみ） | src/HomeFinder.Infrastructure/HomeFinder.Infrastructure.csproj
- [ ] T1-002 [P] セットアップ: バックエンド DotNext NuGet パッケージ（Result<T> サポート）が既にインストール済みか確認し、未インストールの場合のみ追加 | src/HomeFinder.Application/HomeFinder.Application.csproj
- [ ] T1-003 [P] セットアップ: バックエンド Azure.Storage.Blobs NuGet パッケージをインストール | src/HomeFinder.Infrastructure/HomeFinder.Infrastructure.csproj
- [ ] T1-004 [P] セットアップ: フロントエンド Vue 3 HTTP クライアント (axios or fetch) が既に利用可能か確認、追加インストール不要 | src/HomeFinder.UI/package.json
- [ ] T1-005 [P] セットアップ: フロントエンド Vuetify Snackbar コンポーネント確認（未導入の場合はインストール） | src/HomeFinder.UI/package.json
- [ ] T1-006 [P] セットアップ: Docker Azurite ローカル Blob Storage コンテナ起動確認（docker-compose.yml に追加）、Blob コンテナのアクセスレベルを Private に設定（FR-026） | docker-compose.yml
- [ ] T1-007 [P] セットアップ: バックエンド appsettings.Development.json に Azure Blob Storage 接続文字列追加 | src/HomeFinder.Api/appsettings.Development.json

---

### フェーズ 2: 基礎 (ブロッキング前提条件)

DB コンテキスト、Entity Framework Core 設定、DI 登録、ベースとなるエンティティ定義。

- [ ] T2-001 [P] 基礎: バックエンド Image エンティティを Core 層に作成 | src/HomeFinder.Core/Entities/Image.cs
- [ ] T2-002 [P] 基礎: バックエンド DbContext に Image DbSet を追加 | src/HomeFinder.Infrastructure/Data/AppDbContext.cs
- [ ] T2-003 [P] 基礎: バックエンド Item エンティティに ImageId (Guid?) と Navigation プロパティ Image を追加 | src/HomeFinder.Core/Entities/Item.cs
- [ ] T2-003b [P] 基礎: バックエンド AppDbContext.OnModelCreating で Item↔Image の One-to-One 関係設定（HasOne / WithOne / Cascade Delete） | src/HomeFinder.Infrastructure/Data/AppDbContext.cs
- [ ] T2-004 [P] 基礎: バックエンド EF Core マイグレーション作成 (AddMigration AddImageEntity) | src/HomeFinder.Infrastructure/Data/Migrations/
- [ ] T2-005 [P] 基礎: バックエンド EF Core マイグレーション実行テスト (SQL Server スキーマ検証) | src/HomeFinder.Infrastructure/Data/Migrations/
- [ ] T2-006 [P] 基礎: バックエンド IImageRepository インターフェース定義（Application 層）| src/HomeFinder.Application/Repositories/IImageRepository.cs
- [ ] T2-007 [P] 基礎: バックエンド ImageRepository 実装（Infrastructure 層） | src/HomeFinder.Infrastructure/Repositories/ImageRepository.cs
- [ ] T2-008 [P] 基礎: バックエンド Program.cs に ImageRepository DI 登録 | src/HomeFinder.Api/Program.cs
- [ ] T2-009 [P] 基礎: バックエンド IBlobStorageService インターフェース定義（Application 層）— UploadAsync / DownloadAsync / DeleteAsync メソッド | src/HomeFinder.Application/Services/IBlobStorageService.cs
- [ ] T2-010 [P] 基礎: バックエンド AzureBlobStorageService 実装（Infrastructure 層、Azure.Storage.Blobs SDK を使用）| src/HomeFinder.Infrastructure/Services/AzureBlobStorageService.cs
- [ ] T2-011 [P] 基礎: バックエンド IImageProcessor インターフェース定義（Application 層）— ResizeAsync / GetDimensionsAsync メソッド | src/HomeFinder.Application/Services/IImageProcessor.cs
- [ ] T2-012 [P] 基礎: バックエンド ImageSharpImageProcessor 実装（Infrastructure 層、SixLabors.ImageSharp を使用）| src/HomeFinder.Infrastructure/Services/ImageSharpImageProcessor.cs
- [ ] T2-013 [P] 基礎: バックエンド Program.cs に IBlobStorageService / IImageProcessor DI 登録 | src/HomeFinder.Api/Program.cs

---

### フェーズ 3: US1 - 画像アップロード [P1]

Image エンティティ、ImageService、Upload API エンドポイント実装。バックエンド側の完全な検証・保存・置き換え処理。

- [ ] T3-001 [US1] バックエンド: ImageUploadRequest DTO 作成（Application 層 Contracts）| src/HomeFinder.Application/Contracts/ImageUploadRequest.cs
- [ ] T3-002 [US1] バックエンド: ImageUploadResponse DTO 作成（Application 層 Contracts）| src/HomeFinder.Application/Contracts/ImageUploadResponse.cs
- [ ] T3-003 [US1] バックエンド: IImageService インターフェース定義（UploadImageAsync メソッド）| src/HomeFinder.Application/Services/IImageService.cs
- [ ] T3-004 [US1] バックエンド: ImageService 実装 - ファイル形式検証（jpg/bmp/png/webp/svg） | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T3-005 [US1] バックエンド: ImageService 実装 - ファイルサイズ検証（≤10MB） | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T3-006 [US1] バックエンド: ImageService 実装 - 画像解像度取得（IImageProcessor.GetDimensionsAsync 経由） | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T3-007 [US1] バックエンド: ImageService 実装 - 画像を 1000x1000 にリサイズ（IImageProcessor.ResizeAsync 経由） | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T3-008 [US1] バックエンド: ImageService 実装 - Azure Blob Upload 処理（IBlobStorageService.UploadAsync 経由） | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T3-009 [US1] バックエンド: ImageService 実装 - キャッシュヘッダ設定（Cache-Control: max-age=0 for upload） | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T3-010 [US1] バックエンド: ImageService 実装 - 既存画像置き換え処理（古い Image を論理削除） | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T3-011 [US1] バックエンド: ImageService 実装 - Result<T> エラーハンドリング | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T3-012 [US1] バックエンド: Program.cs に ImageService DI 登録 | src/HomeFinder.Api/Program.cs
- [ ] T3-013 [US1] バックエンド: ImagesController 作成・POST エンドポイント実装 | src/HomeFinder.Api/Controllers/ImagesController.cs
- [ ] T3-014 [US1] バックエンド: ImagesController - リクエストパラメータ検証（itemId, multipart/form-data） | src/HomeFinder.Api/Controllers/ImagesController.cs
- [ ] T3-015 [US1] バックエンド: ImagesController - 編集権限チェック（認可） | src/HomeFinder.Api/Controllers/ImagesController.cs
- [ ] T3-016 [US1] バックエンド: ImagesController - エラーレスポンスマッピング（Result.Error → HTTP Status、FORBIDDEN → 403 Forbidden を含む） | src/HomeFinder.Api/Controllers/ImagesController.cs
- [ ] T3-017 [US1] フロントエンド: ImageUploader.vue コンポーネント作成 | src/HomeFinder.UI/src/components/ImageUploader.vue
- [ ] T3-018 [US1] フロントエンド: ImageUploader - File Input 実装（accept 属性指定） | src/HomeFinder.UI/src/components/ImageUploader.vue
- [ ] T3-019 [US1] フロントエンド: ImageUploader - クライアント側ファイル形式検証 | src/HomeFinder.UI/src/components/ImageUploader.vue
- [ ] T3-020 [US1] フロントエンド: ImageUploader - クライアント側ファイルサイズ検証 | src/HomeFinder.UI/src/components/ImageUploader.vue
- [ ] T3-021 [US1] フロントエンド: ImageUploader - FormData 構築・フェッチ送信 | src/HomeFinder.UI/src/components/ImageUploader.vue
- [ ] T3-022 [US1] フロントエンド: useImageNotification.ts コンポーザブル作成 | src/HomeFinder.UI/src/composables/useImageNotification.ts
- [ ] T3-023 [US1] フロントエンド: useImageNotification - Snackbar 成功通知（緑、3秒） | src/HomeFinder.UI/src/composables/useImageNotification.ts
- [ ] T3-024 [US1] フロントエンド: useImageNotification - Snackbar エラー通知（赤、3秒） | src/HomeFinder.UI/src/composables/useImageNotification.ts
- [ ] T3-025 [US1] フロントエンド: ItemEdit.vue および ItemRegister.vue（新規登録ページ）に ImageUploader コンポーネント統合 | src/HomeFinder.UI/src/pages/ItemEdit.vue, src/HomeFinder.UI/src/pages/ItemRegister.vue
- [ ] T3-026 [US1] 契約テスト: ImageUploadApiContract.cs - Upload 成功シナリオ | src/tests/contract/ImageUploadApiContract.cs
- [ ] T3-027 [US1] 契約テスト: ImageUploadApiContract.cs - ファイル形式エラーテスト | src/tests/contract/ImageUploadApiContract.cs
- [ ] T3-028 [US1] 契約テスト: ImageUploadApiContract.cs - ファイルサイズエラーテスト | src/tests/contract/ImageUploadApiContract.cs
- [ ] T3-029 [US1] 契約テスト: ImageUploadApiContract.cs - 解像度エラーテスト | src/tests/contract/ImageUploadApiContract.cs

---

### フェーズ 4: US2 - 詳細ページでの画像表示 [P1]

GET /api/items/{itemId}/image エンドポイント実装、ImagePreview コンポーネント、詳細ページ統合。

- [ ] T4-001 [US2] バックエンド: ImageRetrievalResponse DTO 作成 | src/HomeFinder.Application/Contracts/ImageRetrievalResponse.cs
- [ ] T4-002 [US2] バックエンド: IImageService - GetImageByItemIdAsync メソッド追加 | src/HomeFinder.Application/Services/IImageService.cs
- [ ] T4-003 [US2] バックエンド: ImageService - GetImageByItemIdAsync 実装 | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T4-004 [US2] バックエンド: ImageService - Azure Blob Download 処理 | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T4-005 [US2] バックエンド: ImageService - Cache-Control: max-age=86400 ヘッダ設定 | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T4-006 [US2] バックエンド: ImagesController - GET エンドポイント実装 | src/HomeFinder.Api/Controllers/ImagesController.cs
- [ ] T4-007 [US2] バックエンド: ImagesController - 表示権限チェック（認可） | src/HomeFinder.Api/Controllers/ImagesController.cs
- [ ] T4-008 [US2] バックエンド: ImagesController - IMAGE_NOT_FOUND (404) 処理 | src/HomeFinder.Api/Controllers/ImagesController.cs
- [ ] T4-009 [US2] バックエンド: ImagesController - バイナリ レスポンス返却（Content-Type, Cache-Control）、FORBIDDEN → 403 Forbidden マッピング | src/HomeFinder.Api/Controllers/ImagesController.cs
- [ ] T4-009b [US2] バックエンド: ImagesController - ETag ヘッダ生成（uploadedAtUtc ベース）および If-None-Match に対する 304 Not Modified 返却 | src/HomeFinder.Api/Controllers/ImagesController.cs
- [ ] T4-010 [US2] フロントエンド: imageService.ts 作成（API 呼び出しサービス） | src/HomeFinder.UI/src/services/imageService.ts
- [ ] T4-011 [US2] フロントエンド: imageService - getImageByItemId メソッド実装 | src/HomeFinder.UI/src/services/imageService.ts
- [ ] T4-012 [US2] フロントエンド: ImagePreview.vue コンポーネント作成 | src/HomeFinder.UI/src/components/ImagePreview.vue
- [ ] T4-013 [US2] フロントエンド: ImagePreview - 画像表示（600x600px、object-fit: contain） | src/HomeFinder.UI/src/components/ImagePreview.vue
- [ ] T4-014 [US2] フロントエンド: ImagePreview - プレースホルダー画像表示（画像なし時） | src/HomeFinder.UI/src/components/ImagePreview.vue
- [ ] T4-015 [US2] フロントエンド: ImagePreview - ローディング状態・エラー状態表示 | src/HomeFinder.UI/src/components/ImagePreview.vue
- [ ] T4-016 [US2] フロントエンド: ItemDetail.vue に ImagePreview コンポーネント統合 | src/HomeFinder.UI/src/pages/ItemDetail.vue
- [ ] T4-017 [US2] フロントエンド: ItemDetail.vue - 画像取得フロー実装 | src/HomeFinder.UI/src/pages/ItemDetail.vue
- [ ] T4-018 [US2] 契約テスト: ImageRetrievalApiContract.cs - 成功シナリオ | src/tests/contract/ImageRetrievalApiContract.cs
- [ ] T4-019 [US2] 契約テスト: ImageRetrievalApiContract.cs - IMAGE_NOT_FOUND テスト | src/tests/contract/ImageRetrievalApiContract.cs
- [ ] T4-020 [US2] 契約テスト: ImageRetrievalApiContract.cs - 認可チェックテスト | src/tests/contract/ImageRetrievalApiContract.cs

---

### フェーズ 5: US3 - 一覧ページでの画像表示 [P2]

ItemList.vue での画像プレビュー統合（80x80px）、バルク画像ロード最適化。

- [ ] T5-001 [P] [US3] フロントエンド: imageService - getImagesByItemIds メソッド追加（バルク取得） | src/HomeFinder.UI/src/services/imageService.ts
- [ ] T5-002 [P] [US3] フロントエンド: ImageThumbnail.vue コンポーネント作成 | src/HomeFinder.UI/src/components/ImageThumbnail.vue
- [ ] T5-003 [P] [US3] フロントエンド: ImageThumbnail - 画像表示（80x80px、object-fit: contain） | src/HomeFinder.UI/src/components/ImageThumbnail.vue
- [ ] T5-004 [P] [US3] フロントエンド: ImageThumbnail - プレースホルダー表示 | src/HomeFinder.UI/src/components/ImageThumbnail.vue
- [ ] T5-005 [P] [US3] フロントエンド: ItemList.vue にバルク画像ロード処理実装 | src/HomeFinder.UI/src/pages/ItemList.vue
- [ ] T5-006 [P] [US3] フロントエンド: ItemList.vue の各行に ImageThumbnail 統合 | src/HomeFinder.UI/src/pages/ItemList.vue
- [ ] T5-007 [P] [US3] フロントエンド: ItemList - 一覧パフォーマンス検証（100 件 2 秒以内） | src/HomeFinder.UI/src/pages/ItemList.vue
- [ ] T5-008 [P] [US3] 契約テスト: ListImageDisplayContract.cs - 複数アイテム表示テスト | src/tests/contract/ListImageDisplayContract.cs

---

### フェーズ 6: US4 - 画像削除 [P2]

DELETE /api/items/{itemId}/image エンドポイント実装、削除確認ダイアログ、DeleteImageButton コンポーネント。

- [ ] T6-001 [US4] バックエンド: IImageService - DeleteImageByItemIdAsync メソッド追加 | src/HomeFinder.Application/Services/IImageService.cs
- [ ] T6-002 [US4] バックエンド: ImageService - DeleteImageByItemIdAsync 実装（論理削除） | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T6-003 [US4] バックエンド: ImageService - Azure Blob DeleteBlobAsync 処理 | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T6-004 [US4] バックエンド: ImageService - Item.imageId を NULL に設定 | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T6-005 [US4] バックエンド: ImageService - deletedAtUtc をセット | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T6-006 [US4] バックエンド: ImagesController - DELETE エンドポイント実装 | src/HomeFinder.Api/Controllers/ImagesController.cs
- [ ] T6-007 [US4] バックエンド: ImagesController - 編集権限チェック（認可） | src/HomeFinder.Api/Controllers/ImagesController.cs
- [ ] T6-008 [US4] バックエンド: ImagesController - 204 No Content + Cache-Control: max-age=0 返却 | src/HomeFinder.Api/Controllers/ImagesController.cs
- [ ] T6-009 [US4] フロントエンド: imageService - deleteImage メソッド実装 | src/HomeFinder.UI/src/services/imageService.ts
- [ ] T6-010 [US4] フロントエンド: DeleteImageButton.vue コンポーネント作成 | src/HomeFinder.UI/src/components/DeleteImageButton.vue
- [ ] T6-011 [US4] フロントエンド: DeleteImageButton - 削除確認ダイアログ実装 | src/HomeFinder.UI/src/components/DeleteImageButton.vue
- [ ] T6-012 [US4] フロントエンド: DeleteImageButton - 削除 API 呼び出し実装 | src/HomeFinder.UI/src/components/DeleteImageButton.vue
- [ ] T6-013 [US4] フロントエンド: DeleteImageButton - 成功時スナックバー通知 | src/HomeFinder.UI/src/components/DeleteImageButton.vue
- [ ] T6-014 [US4] フロントエンド: DeleteImageButton - エラー時スナックバー通知 | src/HomeFinder.UI/src/components/DeleteImageButton.vue
- [ ] T6-015 [US4] フロントエンド: ItemDetail.vue に DeleteImageButton 統合 | src/HomeFinder.UI/src/pages/ItemDetail.vue
- [ ] T6-016 [US4] フロントエンド: ItemDetail.vue - 削除後の画像再表示・プレースホルダー切り替え | src/HomeFinder.UI/src/pages/ItemDetail.vue
- [ ] T6-017 [US4] 契約テスト: ImageDeletionApiContract.cs - 成功シナリオ | src/tests/contract/ImageDeletionApiContract.cs
- [ ] T6-018 [US4] 契約テスト: ImageDeletionApiContract.cs - IMAGE_NOT_FOUND テスト | src/tests/contract/ImageDeletionApiContract.cs
- [ ] T6-019 [US4] 契約テスト: ImageDeletionApiContract.cs - 認可チェックテスト | src/tests/contract/ImageDeletionApiContract.cs

---

### フェーズ 7: テスト (契約テスト・統合テスト・コンポーネントテスト)

エンドツーエンドシナリオ、パフォーマンス検証、エッジケース。

- [ ] T7-001 [P] テスト: 統合テスト ImageUploadIntegration.cs - フロー全体（アップロード→取得→削除） | src/tests/integration/ImageUploadIntegration.cs
- [ ] T7-002 [P] テスト: 統合テスト ImageUploadIntegration - ネットワーク接続失敗時のリトライ | src/tests/integration/ImageUploadIntegration.cs
- [ ] T7-003 [P] テスト: 統合テスト ImageUploadIntegration - 同時アップロード処理（複数 API 呼び出し） | src/tests/integration/ImageUploadIntegration.cs
- [ ] T7-004 [P] テスト: 統合テスト ImageUploadIntegration - 既存画像置き換え処理 | src/tests/integration/ImageUploadIntegration.cs
- [ ] T7-005 [P] テスト: 統合テスト ImageUploadIntegration - キャッシュヘッダ検証 | src/tests/integration/ImageUploadIntegration.cs
- [ ] T7-006 [P] テスト: 統合テスト ImageUploadIntegration - DB カスケード削除テスト（Item 削除時に Image も削除） | src/tests/integration/ImageUploadIntegration.cs
- [ ] T7-007 [P] テスト: コンポーネントテスト ImageUploader.spec.ts - ファイル選択 | src/HomeFinder.UI/src/tests/unit/ImageUploader.spec.ts
- [ ] T7-008 [P] テスト: コンポーネントテスト ImageUploader.spec.ts - クライアント側検証 | src/HomeFinder.UI/src/tests/unit/ImageUploader.spec.ts
- [ ] T7-009 [P] テスト: コンポーネントテスト ImageUploader.spec.ts - アップロード成功・失敗フロー | src/HomeFinder.UI/src/tests/unit/ImageUploader.spec.ts
- [ ] T7-010 [P] テスト: コンポーネントテスト ImagePreview.spec.ts - 画像表示・プレースホルダー | src/HomeFinder.UI/src/tests/unit/ImagePreview.spec.ts
- [ ] T7-011 [P] テスト: コンポーネントテスト ImagePreview.spec.ts - ローディング・エラー状態 | src/HomeFinder.UI/src/tests/unit/ImagePreview.spec.ts
- [ ] T7-012 [P] テスト: コンポーネントテスト DeleteImageButton.spec.ts - 削除確認ダイアログ | src/HomeFinder.UI/src/tests/unit/DeleteImageButton.spec.ts
- [ ] T7-013 [P] テスト: コンポーネントテスト DeleteImageButton.spec.ts - 削除フロー | src/HomeFinder.UI/src/tests/unit/DeleteImageButton.spec.ts
- [ ] T7-014 [P] テスト: パフォーマンス検証 - 一覧ページ 100 件レンダリング <= 2 秒 | src/HomeFinder.UI/src/pages/ItemList.vue
- [ ] T7-015 [P] テスト: パフォーマンス検証 - 詳細ページ画像取得・表示 <= 3 秒 | src/HomeFinder.UI/src/pages/ItemDetail.vue
- [ ] T7-016 [P] テスト: パフォーマンス検証 - アップロード処理 <= 3 秒 | src/HomeFinder.Api/Controllers/ImagesController.cs

---

### フェーズ 8: ポーリッシュ (エラーハンドリング洗練・パフォーマンス最適化・ドキュメント)

エッジケース、詳細エラーメッセージ、ドキュメント整理。

- [ ] T8-001 [P] ポーリッシュ: バックエンド エラーメッセージの詳細化（日本語、ユーザーフレンドリー） | src/HomeFinder.Api/Errors/
- [ ] T8-002 [P] ポーリッシュ: バックエンド エッジケース - 削除中のネットワーク失敗（Blob 削除成功、DB 失敗） | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T8-003 [P] ポーリッシュ: バックエンド エッジケース - 同時削除リクエスト（2 番目は IMAGE_NOT_FOUND） | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T8-004 [P] ポーリッシュ: バックエンド ロギング・トレース情報追加（アップロード・削除操作） | src/HomeFinder.Application/Services/ImageService.cs
- [ ] T8-005 [P] ポーリッシュ: フロントエンド 詳細エラーメッセージ表示（モーダルダイアログ） | src/HomeFinder.UI/src/composables/useImageNotification.ts
- [ ] T8-006 [P] ポーリッシュ: フロントエンド アップロード進捗表示（プログレスバー） | src/HomeFinder.UI/src/components/ImageUploader.vue
- [ ] T8-007 [P] ポーリッシュ: フロントエンド ブラウザキャッシュの無効化タイミング（deleteAtUtc 後） | src/HomeFinder.UI/src/pages/ItemDetail.vue
- [ ] T8-008 [P] ポーリッシュ: ドキュメント API 仕様ドキュメント最終確認（contracts/ との同期） | specs/007-item-image-upload/contracts/
- [ ] T8-009 [P] ポーリッシュ: ドキュメント README.md に機能概要・使用方法追加 | README.md
- [ ] T8-010 [P] ポーリッシュ: ドキュメント Swagger/OpenAPI ドキュメント生成（Image エンドポイント） | src/HomeFinder.Api/

---

## 成功基準チェックリスト

各成功基準（SC-001 ～ SC-007）の検証タスク：

- [ ] SC-001 測定: ユーザーが 5 分以内にアイテムへの画像登録を完了 | 手動テスト
- [ ] SC-002 測定: 画像アップロード・表示処理が 3 秒以内に完了 | パフォーマンステスト (T7-016)
- [ ] SC-003 測定: アップロード検証エラーが 100% 正確に検出 | 契約テスト (T3-027 ～ T3-029)
- [ ] SC-004 測定: 100 件アイテム一覧を 2 秒以内にレンダリング | パフォーマンステスト (T7-014)
- [ ] SC-005 測定: 初回試行で画像アップロード成功率 95% 以上 | 統合テスト (T7-001)
- [ ] SC-006 測定: 詳細ページ画像を 600x600px 以下で表示、アスペクト比保持 | コンポーネントテスト (T7-010)
- [ ] SC-007 測定: 一覧ページ画像を 80x80px で統一表示、クライアント中央配置 | コンポーネントテスト (T7-010)

---

## タスク統計

| フェーズ | 区分 | タスク数 |
|---------|------|---------|
| フェーズ 1 | セットアップ | 7 |
| フェーズ 2 | 基礎 | 9 |
| フェーズ 3 | US1 (Upload P1) | 30 |
| フェーズ 4 | US2 (Detail Display P1) | 20 |
| フェーズ 5 | US3 (List Display P2) | 8 |
| フェーズ 6 | US4 (Delete P2) | 19 |
| フェーズ 7 | テスト | 16 |
| フェーズ 8 | ポーリッシュ | 10 |
| **成功基準** | **検証** | **7** |
| **合計** | | **126 タスク** |

### ユーザーストーリー別タスク数

| ユーザーストーリー | 優先度 | タスク数 |
|------------------|--------|---------|
| US1 - Upload | P1 | 30 |
| US2 - Detail Display | P1 | 20 |
| US3 - List Display | P2 | 8 |
| US4 - Delete | P2 | 19 |
| Setup + Foundational | - | 16 |
| Testing | - | 16 |
| Polish | - | 10 |
| **合計** | | **119 タスク** |

---

## 実装推奨順序

### MVP スコープ (Phase 3-4 完了時点)

**フェーズ 1-4 完了**: US1 (Upload) + US2 (Detail Display) の P1 機能  
**推定時間**: 1 週間  
**デリバリー価値**: ユーザーがアイテムに画像をアップロード・閲覧可能

### 追加機能 (Phase 5-6 完了時点)

**フェーズ 5-6 完了**: US3 (List Display) + US4 (Delete) の P2 機能  
**推定時間**: 1 週間  
**デリバリー価値**: 一覧でのプレビュー表示、削除機能

### 完全版 (Phase 7-8 完了時点)

**フェーズ 7-8 完了**: 全テスト、パフォーマンス最適化、ドキュメント  
**推定時間**: 1 週間  
**デリバリー価値**: 本番環境対応、完全なテストカバレッジ

---

## 次のステップ

1. **フェーズ 1 開始**: NuGet/npm パッケージのインストール、Docker 環境確認
2. **フェーズ 2 開始**: Image エンティティ・DbContext・Repository 基盤構築
3. **並列実行**: フェーズ 3-4 (US1/US2) と フェーズ 5-6 (US3/US4) を同時進行
4. **テスト実行**: フェーズ 7 ですべてのテストスイート実行
5. **ポーリッシュ**: フェーズ 8 でドキュメント・エッジケース対応

---

**最終更新**: 2026-05-04
