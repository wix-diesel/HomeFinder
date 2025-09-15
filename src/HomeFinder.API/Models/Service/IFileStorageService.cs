namespace HomeFinderAPI.Models.Service
{
    public interface IFileStorageService
    {
        Task<string> UploadAsync(string fileName, Stream content);
        Task DeleteAsync(string fileName);
    }
}
