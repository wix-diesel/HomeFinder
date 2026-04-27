using System.IO;

namespace ContractTests;

public class StorageLocationsApiContractTests
{
    [Fact]
    public void StorageLocationsApiContract_MustDefineExpectedEndpoints()
    {
        var text = LoadContract("storage-locations-api.md");

        Assert.Contains("**Endpoint**: `GET /api/rooms`", text);
        Assert.Contains("**Endpoint**: `POST /api/rooms`", text);
        Assert.Contains("**Endpoint**: `PUT /api/rooms/{id}`", text);
        Assert.Contains("**Endpoint**: `DELETE /api/rooms/{id}`", text);
        Assert.Contains("**Endpoint**: `GET /api/rooms/{roomId}/shelves`", text);
        Assert.Contains("**Endpoint**: `POST /api/rooms/{roomId}/shelves`", text);
        Assert.Contains("**Endpoint**: `PUT /api/rooms/{roomId}/shelves/{id}`", text);
        Assert.Contains("**Endpoint**: `DELETE /api/rooms/{roomId}/shelves/{id}`", text);
        Assert.Contains("**Response: 200 OK**", text);
        Assert.Contains("**Response: 201 Created**", text);
        Assert.Contains("**Response: 204 No Content**", text);
        Assert.Contains("400 Bad Request", text);
        Assert.Contains("404 Not Found", text);
        Assert.Contains("409 Conflict", text);
    }

    [Fact]
    public void StorageLocationsApiContract_MustDefineErrorCodes()
    {
        var text = LoadContract("storage-locations-api.md");

        Assert.Contains("VALIDATION_ERROR", text);
        Assert.Contains("DUPLICATE_ROOM_NAME", text);
        Assert.Contains("DUPLICATE_SHELF_NAME", text);
        Assert.Contains("ROOM_NOT_FOUND", text);
        Assert.Contains("SHELF_NOT_FOUND", text);
        Assert.Contains("ROOM_HAS_ITEMS", text);
        Assert.Contains("SHELF_HAS_ITEMS", text);
    }

    private static string LoadContract(string filename)
    {
        var path = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory,
            "..", "..", "..", "..", "..", "..",
            "specs", "005-storage-location-management", "contracts", filename));

        return File.ReadAllText(path);
    }
}
