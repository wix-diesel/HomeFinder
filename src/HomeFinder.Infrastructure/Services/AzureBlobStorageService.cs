using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using HomeFinder.Application.Services;
using HomeFinder.Core.Errors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HomeFinder.Infrastructure.Services;

/// <summary>
/// Azure Blob Storage を使用した IBlobStorageService の実装（Infrastructure 層）
/// </summary>
public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;
        var connectionString = configuration.GetConnectionString("AzureBlobStorage")
            ?? throw new InvalidOperationException("AzureBlobStorage 接続文字列が設定されていません。");
        var containerName = configuration["AzureBlobStorage:ContainerName"] ?? "images";
        _containerClient = new BlobContainerClient(connectionString, containerName);
    }

    /// <inheritdoc />
    public async Task<string> UploadAsync(
        string blobName,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // コンテナが存在しない場合は作成（Private アクセスレベル）
            await _containerClient.CreateIfNotExistsAsync(
                PublicAccessType.None,
                cancellationToken: cancellationToken);

            var blobClient = _containerClient.GetBlobClient(blobName);
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType }
            };

            await blobClient.UploadAsync(content, uploadOptions, cancellationToken);

            _logger.LogInformation("Blob アップロード完了: {BlobName}", blobName);
            return blobClient.Uri.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Blob アップロード失敗: {BlobName}", blobName);
            throw new BlobStorageException("Upload", ex);
        }
    }

    /// <inheritdoc />
    public async Task<(Stream Content, string ContentType)> DownloadAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            var response = await blobClient.DownloadStreamingAsync(cancellationToken: cancellationToken);

            var contentType = response.Value.Details.ContentType ?? "application/octet-stream";
            _logger.LogInformation("Blob ダウンロード完了: {BlobName}", blobName);
            return (response.Value.Content, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Blob ダウンロード失敗: {BlobName}", blobName);
            throw new BlobStorageException("Download", ex);
        }
    }

    /// <inheritdoc />
    public async Task DeleteAsync(
        string blobName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            _logger.LogInformation("Blob 削除完了: {BlobName}", blobName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Blob 削除失敗: {BlobName}", blobName);
            throw new BlobStorageException("Delete", ex);
        }
    }
}
