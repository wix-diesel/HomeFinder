# クイックスタート: アイテム画像アップロード

**機能**: 007-item-image-upload | **日付**: 2026-05-04 | **関連**: [plan.md](plan.md)

## ローカル開発環境セットアップ

### 前提条件

- Docker & Docker Compose がインストール済み
- .NET 10 SDK がインストール済み
- Node.js 18+ & pnpm がインストール済み
- Visual Studio Code 推奨

### Step 1: リポジトリクローン＆ディレクトリ移動

```bash
cd c:\repo\HomeFinder
```

### Step 2: Docker Compose で インフラ起動

```bash
docker compose up -d
```

**確認コマンド**:
```bash
docker compose ps
```

期待される出力:
```
CONTAINER ID   IMAGE                      STATUS
xxx            azure-storage-emulator     Up 2 minutes (port 10000)
xxx            mcr.microsoft.com/mssql    Up 2 minutes (port 1433)
```

### Step 3: DB マイグレーション実行

```bash
cd src/HomeFinder.Infrastructure

# 新しいマイグレーション作成
dotnet ef migrations add AddImageEntity --context AppDbContext

# DB に反映
dotnet ef database update --context AppDbContext

cd ../..
```

**確認**: SQL Server に `Images` テーブルが作成されたことを確認
```sql
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Images';
```

### Step 4: フロントエンド依存関係インストール

```bash
cd src/HomeFinder.UI

pnpm install

cd ../..
```

### Step 5: 開発サーバー起動

#### ターミナル A: バックエンド

```bash
cd src/HomeFinder.Api
dotnet run
```

期待される出力:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
      Now listening on: http://localhost:5001
```

#### ターミナル B: フロントエンド

```bash
cd src/HomeFinder.UI
pnpm run dev
```

期待される出力:
```
VITE v4.x.x ready in xxx ms

➜  Local:   http://localhost:5173/
```

---

## 最初のテスト実行

### 1️⃣ 契約テスト (Backend)

```bash
cd src/tests/contract

# xUnit テスト実行
dotnet test ImageApiContract.cs -v normal
```

期待される結果:
```
✓ TestImageUploadSuccess
✓ TestImageUploadInvalidFormat
✓ TestImageUploadFileTooLarge
✓ TestImageRetrievalSuccess
✓ TestImageDeletionSuccess
```

### 2️⃣ フロントエンド ユニットテスト

```bash
cd src/HomeFinder.UI

# Vitest テスト実行
pnpm run test
```

期待される結果:
```
✓ ImageUploader.spec.ts (5)
✓ ImagePreview.spec.ts (3)
✓ useImageNotification.spec.ts (2)
```

### 3️⃣ 統合テスト (Backend)

```bash
cd src/tests/integration

# 実際のアップロード・取得・削除フロー
dotnet test ImageUploadIntegration.cs -v normal
```

期待される結果:
```
✓ TestUploadAndRetrieveImage
✓ TestUploadAndReplaceImage
✓ TestUploadAndDeleteImage
```

---

## ユーザーストーリー動作確認フロー

### Story 1: 画像アップロード (P1)

#### 準備

1. ブラウザで `http://localhost:5173` にアクセス
2. アイテムの追加/編集ページに遷移

#### 実行ステップ

```
1. [ImageUploader] コンポーネント をレンダリング確認
   └─ ファイル選択ボタン表示

2. [File Input] でローカルの JPEG / PNG ファイルを選択
   └─ ファイル情報がコンポーネント状態に保存される

3. [アップロード] ボタンをクリック
   └─ FormData を POST /api/items/{itemId}/image に送信

4. [バックエンド処理]
   └─ ファイル検証 (形式・サイズ・解像度)
   └─ 1000x1000 に正規化
   └─ Azure Blob に保存
   └─ Image エンティティを DB に作成
   └─ Item.imageId を更新
   └─ 200 OK + { imageId, blobUri }

5. [スナックバー] 表示（緑色, "画像がアップロードされました"）
   └─ 3 秒後に自動消滅
```

#### 検証ポイント

- ✅ スナックバー: 成功メッセージ（緑色）
- ✅ DB: Image レコード作成確認
- ✅ Azure Blobs: ファイル存在確認
- ✅ Item: imageId が更新されたことを確認

```bash
# DB 確認
SELECT Id, ItemId, FileName, FileFormat, UploadedAtUtc 
FROM Images 
WHERE ItemId = '{itemId}' AND DeletedAtUtc IS NULL;

# Azure Blob 確認（Azurite）
curl http://localhost:10000/devstoreaccount1/images -H "x-ms-version: 2019-12-12"
```

---

### Story 2: 詳細ページ画像表示 (P1)

#### 準備

- Story 1 で画像がアップロードされた状態

#### 実行ステップ

```
1. アイテム詳細ページ（ItemDetail.vue）を開く

2. [ImagePreview] コンポーネント レンダリング
   └─ Item.imageId が存在する場合、GET /api/items/{itemId}/image を実行
   └─ バイナリ画像データを取得

3. [Canvas / <img>] で画像表示
   └─ CSS: max-width: 600px; height: 600px; object-fit: contain;
   └─ 中央配置で表示

4. ブラウザキャッシュ確認
   └─ Cache-Control: max-age=86400
   └─ 2 回目アクセス時 304 Not Modified
```

#### 検証ポイント

- ✅ 画像が 600x600px 以下で表示されている
- ✅ アスペクト比が保持されている（正方形でない場合は中央配置）
- ✅ ネットワークタブで Cache-Control ヘッダ確認
- ✅ 2 回目アクセス時 304 返却

---

### Story 3: 一覧ページ画像表示 (P2)

#### 準備

- 複数のアイテムを作成
- うち半数に画像をアップロード

#### 実行ステップ

```
1. アイテム一覧ページ（ItemList.vue）を開く

2. 各アイテム行に [ImagePreview] コンポーネント（80x80px）を表示
   ├─ 画像あり: GET /api/items/{id}/image で取得・表示
   └─ 画像なし: プレースホルダー画像表示

3. ページレンダリング時間を計測
   └─ 期待値: 2 秒以内（100 件の一覧）

4. ブラウザキャッシュ動作
   └─ 初回ロード: 複数の画像 GET リクエスト
   └─ 再ロード: キャッシュから即座に表示
```

#### 検証ポイント

- ✅ 画像が 80x80px で表示されている
- ✅ プレースホルダーが正しく表示されている
- ✅ DevTools Performance タブで総レンダリング時間が 2 秒以内
- ✅ ネットワークタブで重複リクエストがない（キャッシュ有効）

---

### Story 4: 画像削除 (P2)

#### 準備

- Story 1 で画像がアップロードされた状態

#### 実行ステップ

```
1. アイテム詳細ページで [画像削除] ボタン クリック
   └─ [DeleteImageButton] コンポーネント

2. 確認ダイアログ表示
   └─ "この画像を削除してもよろしいですか？"

3. [確認] ボタン クリック
   └─ DELETE /api/items/{itemId}/image を実行

4. [バックエンド処理]
   └─ 認可チェック
   └─ Azure Blob から削除
   └─ Image を論理削除 (deletedAtUtc = now())
   └─ Item.imageId を NULL に更新
   └─ 204 No Content

5. [スナックバー] 表示（緑色, "画像が削除されました"）
   └─ 3 秒後に自動消滅

6. [ImagePreview] コンポーネント プレースホルダーに切り替わる
```

#### 検証ポイント

- ✅ 確認ダイアログが表示された
- ✅ スナックバー: 成功メッセージ（緑色）
- ✅ DB: Image.deletedAtUtc が セット、Item.imageId が NULL
- ✅ Azure Blob: ファイルが削除された
- ✅ UI: プレースホルダー画像に切り替わった

```bash
# DB 確認
SELECT Id, ItemId, DeletedAtUtc FROM Images WHERE ItemId = '{itemId}';
SELECT Id, ImageId FROM Items WHERE Id = '{itemId}';

# Azure Blob 確認（削除後）
curl http://localhost:10000/devstoreaccount1/images -H "x-ms-version: 2019-12-12"
```

---

## エラーハンドリングテスト

### T1: 無効な画像形式アップロード

```
1. Story 1 のステップで WebM 動画ファイルを選択
2. [スナックバー] 赤色 "ファイル形式が無効です..."
3. DB: Image レコード作成されない
```

### T2: 10MB超過ファイルアップロード

```
1. Story 1 のステップで 15MB JPEG ファイルを選択
2. [スナックバー] 赤色 "ファイルサイズが 10MB を超えています..."
3. DB: Image レコード作成されない
```

### T3: ネットワーク障害時の削除

```
1. Chrome DevTools Network タブ で "Offline" にチェック
2. Story 4 の削除実行
3. [スナックバー] 赤色 "削除処理に失敗しました..."
4. DB: Item.imageId は変更されない（削除されない）
```

---

## トラブルシューティング

### Docker コンテナが起動しない

```bash
# コンテナログ確認
docker compose logs azurite-blob

# 強制再起動
docker compose down -v
docker compose up -d
```

### DB マイグレーション失敗

```bash
# 既存マイグレーション確認
dotnet ef migrations list

# 前のマイグレーションに戻す（必要時）
dotnet ef database update {PreviousMigrationName}
```

### フロントエンド でビルドエラー

```bash
# node_modules クリア＆再インストール
rm -r node_modules pnpm-lock.yaml
pnpm install
```

### ブラウザでブランク表示

```bash
# Dev Server ログ確認
pnpm run dev

# キャッシュクリア
# Chrome DevTools > Application > Clear site data
```

---

## 成功基準チェックリスト

| 基準 | テスト方法 | 期待値 |
|------|----------|--------|
| SC-001: 5 分以内に登録完了 | Story 1 の時間計測 | ✅ 3 分以内 |
| SC-002: 3 秒以内に表示完了 | DevTools Performance | ✅ 2 秒以内 |
| SC-003: 検証 100% 正確 | T1, T2 実行 | ✅ すべてのバリデーション動作 |
| SC-004: 100 件 2 秒レンダリング | Story 3 ページ読み込み | ✅ 1.8 秒 |
| SC-005: 95% 成功率 | 10 回アップロード実行 | ✅ 9-10 回成功 |
| SC-006: 600x600px 以下 | Story 2 CSS 確認 | ✅ max-width: 600px |
| SC-007: 80x80px 統一 | Story 3 CSS 確認 | ✅ max-width: 80px |

---

## 次のステップ

✅ ローカル開発・テスト完了後:

1. `/speckit.tasks` コマンドで **tasks.md** を生成
2. バックログに実装タスクを追加
3. 実装フェーズに移行

```bash
/speckit.tasks
```
