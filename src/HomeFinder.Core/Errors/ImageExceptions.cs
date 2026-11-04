namespace HomeFinder.Core.Errors;

/// <summary>
/// 指定された画像が見つからない場合にスローする。
/// </summary>
public class ImageNotFoundException(Guid imageId) : Exception($"Image not found: {imageId}")
{
    public Guid ImageId { get; } = imageId;
}

/// <summary>
/// アイテムに紐付く画像が見つからない場合にスローする。
/// </summary>
public class ImageNotFoundForItemException(Guid itemId) : Exception($"Image not found for item: {itemId}")
{
    public Guid ItemId { get; } = itemId;
}

/// <summary>
/// 画像操作に必要な権限がない場合にスローする。
/// </summary>
public class ImageForbiddenException(Guid itemId) : Exception($"Image operation forbidden for item: {itemId}")
{
    public Guid ItemId { get; } = itemId;
}

/// <summary>
/// アップロードされた画像のファイル形式が無効な場合にスローする。
/// </summary>
public class ImageInvalidFormatException(string format) : Exception($"Invalid image format: {format}")
{
    public string Format { get; } = format;
}

/// <summary>
/// 保存する画像ファイル名が長さ・文字の制約を満たさない場合にスローする。
/// </summary>
public class ImageInvalidFileNameException(int maxLength)
    : Exception($"Image file name must contain 1 to {maxLength} characters and no control characters.")
{
    public int MaxLength { get; } = maxLength;
}

/// <summary>
/// アップロードされたファイルサイズが制限を超えている場合にスローする。
/// </summary>
public class ImageFileTooLargeException(long fileSizeBytes, long maxSizeBytes)
    : Exception($"Image file too large: {fileSizeBytes} bytes (max: {maxSizeBytes} bytes)")
{
    public long FileSizeBytes { get; } = fileSizeBytes;
    public long MaxSizeBytes { get; } = maxSizeBytes;
}

/// <summary>
/// 画像の解像度が制限を超えている場合にスローする。
/// </summary>
public class ImageInvalidResolutionException(int width, int height, int maxSize)
    : Exception($"Image resolution {width}x{height} exceeds maximum {maxSize}x{maxSize}")
{
    public int Width { get; } = width;
    public int Height { get; } = height;
    public int MaxSize { get; } = maxSize;
}

/// <summary>
/// Blob Storage への操作に失敗した場合にスローする。
/// </summary>
public class BlobStorageException(string operation, Exception? innerException = null)
    : Exception($"Blob storage operation failed: {operation}", innerException)
{
    public string Operation { get; } = operation;
}
