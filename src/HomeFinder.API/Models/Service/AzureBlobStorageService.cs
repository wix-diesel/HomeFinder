using Azure.Storage.Blobs;

namespace HomeFinderAPI.Models.Service
{

    public class AzureBlobStorageService : IFileStorageService
    {
        private readonly BlobContainerClient _container;

        public AzureBlobStorageService(BlobContainerClient container)
        {
            _container = container;
        }

        public async Task<string> UploadAsync(string fileName, Stream content)
        {
            var blobClient = _container.GetBlobClient(fileName);
            await blobClient.UploadAsync(content, overwrite: true);
            return blobClient.Uri.ToString();
        }

        public async Task DeleteAsync(string fileName)
        {
            var blobClient = _container.GetBlobClient(fileName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
