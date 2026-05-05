// 画像アップロードレスポンスの型定義
export type ImageUploadResponse = {
  imageId: string;
  blobUri: string;
  uploadedAtUtc: string;
};

// 画像取得時のメタデータ型定義
export type ImageInfo = {
  imageId: string;
  blobUri: string;
  fileFormat: string;
  fileSizeBytes: number;
  uploadedAtUtc: string;
};
