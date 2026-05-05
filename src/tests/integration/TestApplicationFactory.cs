using HomeFinder.Infrastructure.Data;
using HomeFinder.Application.Services;
using HomeFinder.Core.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Collections.Concurrent;

namespace IntegrationTests;

public class TestApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"HomeFinderTestDb-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ItemDbContext>));
            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            var dbContextOptionsConfigurations = services
                .Where(d => d.ServiceType.IsGenericType
                            && d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>)
                            && d.ServiceType.GenericTypeArguments[0] == typeof(ItemDbContext))
                .ToList();

            foreach (var serviceDescriptor in dbContextOptionsConfigurations)
            {
                services.Remove(serviceDescriptor);
            }

            services.AddDbContext<ItemDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.RemoveAll<IBlobStorageService>();
            services.RemoveAll<IImageProcessor>();
            services.AddSingleton<IBlobStorageService, InMemoryBlobStorageService>();
            services.AddSingleton<IImageProcessor, InMemoryImageProcessor>();

            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ItemDbContext>();
            db.Database.EnsureCreated();

            if (!db.Items.Any())
            {
                db.Items.AddRange(
                    new Item
                    {
                        Id = Guid.NewGuid(),
                        Name = "歯ブラシ",
                        Quantity = 2,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    },
                    new Item
                    {
                        Id = Guid.NewGuid(),
                        Name = "石鹸",
                        Quantity = 1,
                        CreatedAtUtc = DateTime.UtcNow,
                        UpdatedAtUtc = DateTime.UtcNow
                    });

                db.SaveChanges();
            }

            if (!db.Categories.Any())
            {
                db.Categories.AddRange(
                    new Category
                    {
                        Id = Category.Reserved.UnclassifiedId,
                        Name = Category.Reserved.UnclassifiedName,
                        NormalizedName = Category.Reserved.UnclassifiedName,
                        Icon = null,
                        Color = null,
                        IsReserved = true,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    },
                    new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = "食器",
                        NormalizedName = "食器",
                        Icon = "restaurant",
                        Color = "#FF6B6B",
                        IsReserved = false,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    });

                db.SaveChanges();
            }
        });
    }
}

internal sealed class InMemoryBlobStorageService : IBlobStorageService
{
    private static readonly ConcurrentDictionary<string, (byte[] Bytes, string ContentType)> Blobs = new();
    private static int _failUploadAttempts;
    private static int _failDeleteAttempts;

    public static void Reset()
    {
        Blobs.Clear();
        _failUploadAttempts = 0;
        _failDeleteAttempts = 0;
    }

    public static void FailNextUploadAttempts(int count)
    {
        _failUploadAttempts = Math.Max(0, count);
    }

    public static void FailNextDeleteAttempts(int count)
    {
        _failDeleteAttempts = Math.Max(0, count);
    }

    public static bool ContainsBlob(string blobName) => Blobs.ContainsKey(blobName);

    public Task<string> UploadAsync(string blobName, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        if (_failUploadAttempts > 0)
        {
            Interlocked.Decrement(ref _failUploadAttempts);
            throw new InvalidOperationException("Simulated upload failure");
        }

        using var ms = new MemoryStream();
        content.CopyTo(ms);
        Blobs[blobName] = (ms.ToArray(), contentType);
        return Task.FromResult($"http://inmemory.local/images/{blobName}");
    }

    public Task<(Stream Content, string ContentType)> DownloadAsync(string blobName, CancellationToken cancellationToken = default)
    {
        if (!Blobs.TryGetValue(blobName, out var data))
        {
            throw new FileNotFoundException($"Blob not found: {blobName}");
        }

        return Task.FromResult<(Stream, string)>((new MemoryStream(data.Bytes), data.ContentType));
    }

    public Task DeleteAsync(string blobName, CancellationToken cancellationToken = default)
    {
        if (_failDeleteAttempts > 0)
        {
            Interlocked.Decrement(ref _failDeleteAttempts);
            throw new InvalidOperationException("Simulated delete failure");
        }

        Blobs.TryRemove(blobName, out _);
        return Task.CompletedTask;
    }
}

internal sealed class InMemoryImageProcessor : IImageProcessor
{
    public Task<(int Width, int Height)> GetDimensionsAsync(Stream imageStream, CancellationToken cancellationToken = default)
    {
        return Task.FromResult((100, 100));
    }

    public Task<Stream> ResizeAsync(Stream imageStream, int maxWidth, int maxHeight, CancellationToken cancellationToken = default)
    {
        var ms = new MemoryStream();
        imageStream.Position = 0;
        imageStream.CopyTo(ms);
        ms.Position = 0;
        return Task.FromResult<Stream>(ms);
    }
}
