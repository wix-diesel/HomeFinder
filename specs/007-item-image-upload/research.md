# リサーチ: アイテム画像アップロード

**機能**: 007-item-image-upload | **日付**: 2026-05-04 | **関連計画**: [plan.md](plan.md)

## T-001: Azure Blob Storage (Azurite) での画像リサイズ戦略

### 決定事項

**保存解像度**: 1000x1000 ピクセルに正規化（サーバーサイド）  
**配信戦略**: 
- 詳細ページ（600x600px）：CSS の `object-fit: contain` で中央配置、クライアント側スケーリング不要
- 一覧ページ（80x80px）：同様に `object-fit: contain` で中央配置

**Azurite メタデータ**:
```
Content-Type: image/jpeg | image/png | image/webp | image/bmp | image/svg+xml
Cache-Control: max-age=86400
```

**実装方法**:
- SixLabors.ImageSharp を使用したサーバーサイドリサイズ（.NET ライブラリ）
- アップロード時に画像を 1000x1000 に正規化し、Azure Blob に保存
- クライアント側では CSS `max-width: 600px; height: 600px; object-fit: contain; margin: auto;` で表示

**容量・パフォーマンス**:
- 1000x1000 PNG: 平均 150-300KB
- 一覧ページ 100 件：15-30MB 初回ロード、その後ブラウザキャッシュ（max-age=86400）で 1 日有効
- 詳細ページ 1 件：150-300KB、同様にキャッシュ有効

**推奨理由**: 画像を 1 つのサイズで保存することで、変換・キャッシュ管理の複雑性を低減。CSS での表示サイズ制御でレスポンシブ対応。

---

## T-002: DotNext.Result<T> による Azure Blob API のエラー処理

### 決定事項

**Application 層パターン**:

```csharp
// ImageService.cs (Application layer)
public class ImageService : IImageService
{
    private readonly IImageRepository _imageRepository;
    
    public async ValueTask<Result<ImageUploadResponse>> UploadImageAsync(
        Guid itemId, 
        Stream fileStream, 
        string fileName,
        CancellationToken cancellationToken = default)
    {
        // 検証
        if (fileStream.Length > 10 * 1024 * 1024)
            return new Error("FILE_TOO_LARGE", "ファイルサイズが 10MB を超えています");
        
        try
        {
            var image = new Image { ... };
            var result = await _imageRepository.CreateAsync(image, cancellationToken);
            
            return result.IsSuccess 
                ? new ImageUploadResponse { ImageId = image.Id, BlobUri = image.BlobUri }
                : result.AsError<ImageUploadResponse>();
        }
        catch (Exception ex)
        {
            return new Error("UPLOAD_FAILED", $"アップロード処理に失敗しました: {ex.Message}");
        }
    }
}
```

**API 層パターン**:

```csharp
// ImagesController.cs (Api layer)
[HttpPost("{itemId}/image")]
public async Task<IActionResult> UploadImage(Guid itemId, IFormFile image)
{
    var result = await _imageService.UploadImageAsync(itemId, image.OpenReadStream(), image.FileName);
    
    if (!result.IsSuccess)
    {
        return result.Error.Code switch
        {
            "UNAUTHORIZED" => Forbid(),
            "FILE_TOO_LARGE" => BadRequest(new { code = result.Error.Code, message = result.Error.Message }),
            "INVALID_FORMAT" => BadRequest(new { code = result.Error.Code, message = result.Error.Message }),
            _ => StatusCode(500, new { code = "INTERNAL_ERROR", message = result.Error.Message })
        };
    }
    
    return Ok(result.Value);
}
```

**Azure Blob SDK の例外処理**:
- `Azure.RequestFailedException` (404) → IMAGE_NOT_FOUND
- `Azure.RequestFailedException` (403) → UNAUTHORIZED
- `System.IO.IOException` → UPLOAD_FAILED

**推奨理由**: Result<T> を使用することで、例外処理をスキップし、ビジネスロジックと分岐制御が明確になる。API 層で統一的に HTTP ステータスコードに変換可能。

---

## T-003: Vue 3 で File Input + Multipart Form Data の実装

### 決定事項

**Vue 3 + Composition API コンポーネント**:

```typescript
// ImageUploader.vue
<template>
  <div class="image-uploader">
    <input 
      ref="fileInput"
      type="file"
      accept="image/jpeg,image/png,image/webp,image/bmp,image/svg+xml"
      @change="onFileSelect"
    />
    <button @click="uploadImage" :disabled="!selectedFile">
      アップロード
    </button>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue';
import { useImageNotification } from '@/composables/useImageNotification';

const props = defineProps<{ itemId: string }>();
const emit = defineEmits<{ success: [imageId: string] }>();

const fileInput = ref<HTMLInputElement>();
const selectedFile = ref<File>();
const { showSuccess, showError } = useImageNotification();

const onFileSelect = (e: Event) => {
  const files = (e.target as HTMLInputElement).files;
  if (files?.length) {
    selectedFile.value = files[0];
  }
};

const uploadImage = async () => {
  if (!selectedFile.value) return;
  
  const formData = new FormData();
  formData.append('image', selectedFile.value);
  
  try {
    const res = await fetch(`/api/items/${props.itemId}/image`, {
      method: 'POST',
      body: formData
    });
    
    if (res.ok) {
      const { imageId } = await res.json();
      showSuccess('画像がアップロードされました');
      emit('success', imageId);
    } else {
      const { message } = await res.json();
      showError(message);
    }
  } catch (err) {
    showError('アップロード処理に失敗しました');
  }
};
</script>
```

**Snackbar コンポーネント** (Vuetify 導入を想定):
```typescript
// useImageNotification.ts
import { useSnackbar } from 'vuetify';

export const useImageNotification = () => {
  const { add } = useSnackbar();
  
  const showSuccess = (message: string) => {
    add({ text: message, color: 'success', timeout: 3000 });
  };
  
  const showError = (message: string) => {
    add({ text: message, color: 'error', timeout: 3000 });
  };
  
  return { showSuccess, showError };
};
```

**イメージメタデータ取得**:
```typescript
const getImageDimensions = (file: File): Promise<{ width: number; height: number }> => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader();
    reader.onload = (e) => {
      const img = new Image();
      img.onload = () => resolve({ width: img.width, height: img.height });
      img.src = e.target?.result as string;
    };
    reader.readAsDataURL(file);
  });
};
```

**推奨理由**: Composition API + TypeScript で型安全。FormData での multipart/form-data 送信は標準 Fetch API で対応可能。Vuetify の Snackbar で自動消滅（3秒）の通知が簡潔に実装可能。

---

## T-004: Entity Framework Core での Image エンティティ追加・マイグレーション

### 決定事項

**Image エンティティ** (HomeFinder.Core/Entities/Image.cs):

```csharp
public class Image
{
    public Guid Id { get; set; }
    public Guid ItemId { get; set; }
    public string BlobUri { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty; // jpg, png, webp, bmp, svg
    public int FileSizeBytes { get; set; }
    public int OriginalWidth { get; set; }
    public int OriginalHeight { get; set; }
    public DateTime UploadedAtUtc { get; set; }
    public DateTime? DeletedAtUtc { get; set; } // 論理削除用
    
    // Navigation
    public Item Item { get; set; } = null!;
}
```

**Item エンティティ修正** (HomeFinder.Core/Entities/Item.cs):

```csharp
public class Item
{
    // [既存フィールド...]
    public Guid? ImageId { get; set; }
    
    // Navigation
    public Image? Image { get; set; }
}
```

**DbContext 設定** (HomeFinder.Infrastructure/Data/AppDbContext.cs):

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Image>()
        .HasKey(i => i.Id);
    
    modelBuilder.Entity<Image>()
        .Property(i => i.BlobUri)
        .IsRequired()
        .HasMaxLength(2048);
    
    modelBuilder.Entity<Image>()
        .Property(i => i.FileFormat)
        .IsRequired()
        .HasMaxLength(10);
    
    modelBuilder.Entity<Item>()
        .HasOne(x => x.Image)
        .WithOne(i => i.Item)
        .HasForeignKey<Image>(i => i.ItemId);
}
```

**マイグレーション実行**:

```powershell
# リポジトリルート
cd src/HomeFinder.Infrastructure
dotnet ef migrations add AddImageEntity --context AppDbContext
dotnet ef database update --context AppDbContext
```

**DB テーブル構成**:

```sql
CREATE TABLE [dbo].[Images] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ItemId] UNIQUEIDENTIFIER NOT NULL,
    [BlobUri] NVARCHAR(2048) NOT NULL,
    [FileName] NVARCHAR(500) NOT NULL,
    [FileFormat] NVARCHAR(10) NOT NULL,
    [FileSizeBytes] INT NOT NULL,
    [OriginalWidth] INT NOT NULL,
    [OriginalHeight] INT NOT NULL,
    [UploadedAtUtc] DATETIME2 NOT NULL,
    [DeletedAtUtc] DATETIME2,
    CONSTRAINT [FK_Images_Items_ItemId] FOREIGN KEY ([ItemId]) 
        REFERENCES [Items]([Id]) ON DELETE CASCADE
);

ALTER TABLE [Items] ADD [ImageId] UNIQUEIDENTIFIER NULL;
CREATE UNIQUE INDEX [IX_Items_ImageId] ON [Items]([ImageId]) 
WHERE [ImageId] IS NOT NULL;
```

**推奨理由**: One-to-One 関係で Item.ImageId が nullable なため、画像がない状態も表現可能。論理削除（DeletedAtUtc）で復旧可能性を確保。UTC タイムスタンプで タイムゾーン問題を回避。

---

## まとめ

| リサーチ項目 | 結論 | 実装詳細 |
|-----------|------|---------|
| 画像リサイズ戦略 | 1000x1000 正規化 + CSS object-fit | SixLabors.ImageSharp (サーバー側) |
| エラー処理 | Result<T> + HTTP ステータスマッピング | Application 層で Result<T> 返却、API 層で HTTP コード決定 |
| Vue 3 ファイルアップロード | Composition API + FormData | fetch API multipart/form-data、Vuetify Snackbar |
| EF Core エンティティ | One-to-One Item ⟷ Image | DbContext 関係設定、マイグレーション実行 |

**結果**: 🎯 すべてのリサーチ項目が解決。Phase 1（設計・契約）へ進行可能。
