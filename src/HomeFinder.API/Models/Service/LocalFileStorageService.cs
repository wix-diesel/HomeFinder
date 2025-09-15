namespace HomeFinderAPI.Models.Service
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly string _basePath;
        private readonly string _baseUrl;
        private readonly ILogger<LocalFileStorageService> _logger;

        public LocalFileStorageService(IConfiguration config, ILogger<LocalFileStorageService> logger)
        {
            _basePath = config["LocalStorage:BasePath"]
                ?? throw new ArgumentNullException("LocalStorage:BasePath");
            _baseUrl = config["LocalStorage:BaseUrl"]
                ?? throw new ArgumentNullException("LocalStorage:BaseUrl");
            _logger = logger;
        }

        public async Task<string> UploadAsync(string fileName, Stream content)
        {
            // 保存フォルダが存在しなければ作成
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            // ファイルパスを作成
            var fullPath = Path.Combine(_basePath, fileName);

            // IIS上でのURLを作成
            var fileUrl = $"{_baseUrl.TrimEnd('/')}/{fileName}";

            // 保存処理
            using (var fileStream = File.Create(fullPath))
            {
                await content.CopyToAsync(fileStream);
            }

            return fileUrl;
        }
        public Task DeleteAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return Task.CompletedTask;

            // URLからファイル名部分を抽出
            var fileName = Path.GetFileName(new Uri(fileUrl).AbsolutePath);

            var fullPath = Path.Combine(_basePath, fileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }

    }


}
