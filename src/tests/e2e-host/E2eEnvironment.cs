using Azure.Storage.Blobs;
using Microsoft.Data.SqlClient;

namespace HomeFinder.E2eHost;

public static class E2eEnvironment
{
    public static void Validate(IHostEnvironment environment, IConfiguration configuration)
    {
        if (!environment.IsEnvironment("E2E"))
            throw new InvalidOperationException("E2E ホストは E2E 環境でのみ起動できます。");

        var sql = new SqlConnectionStringBuilder(configuration.GetConnectionString("DefaultConnection"));
        if (sql.DataSource != "127.0.0.1,14337" || sql.InitialCatalog != "HomeFinderE2E"
            || sql.IntegratedSecurity || string.IsNullOrWhiteSpace(sql.UserID)
            || sql.Authentication is not (SqlAuthenticationMethod.NotSpecified or SqlAuthenticationMethod.SqlPassword))
            throw new InvalidOperationException("E2E 専用のローカル SQL Server / HomeFinderE2E データベースを指定してください。");

        var blob = new BlobContainerClient(configuration.GetConnectionString("AzureBlobStorage"), "homefinder-e2e");
        if (blob.AccountName != "devstoreaccount1" || !blob.Uri.IsLoopback || blob.Uri.Port != 10017
            || configuration["AzureBlobStorage:ContainerName"] != "homefinder-e2e")
            throw new InvalidOperationException("E2E 専用のローカル Azurite を指定してください。");
    }
}
