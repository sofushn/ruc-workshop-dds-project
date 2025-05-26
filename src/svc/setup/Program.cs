using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ExifLibrary;
using Npgsql;

string waypointsUrl = "https://api.github.com/repos/sofushn/ruc-workshop-dds-project/contents/images/waypoints";
string mapDataUrl = "https://raw.githubusercontent.com/sofushn/ruc-workshop-dds-project/main/images/maps/2dmap.jpg";

string postgresConnectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") 
    ?? throw new ArgumentNullException("POSTGRES_CONNECTION_STRING environment variable is not set");

string imageStoreUrl = Environment.GetEnvironmentVariable("IMAGE_STORE_URL") 
    ?? throw new ArgumentNullException("IMAGE_STORE_URL environment variable is not set");

Console.WriteLine("Environment variables loaded successfully");
Console.WriteLine($"Using Postgres connection string: {postgresConnectionString}");
Console.WriteLine($"Using Image Store URL: {imageStoreUrl}");

HttpClient generalClient = new() {
    DefaultRequestHeaders = {
        { "User-Agent", "RucWorkshopSetup/1.0" }
    }
};

HttpClient imageStoreClient = new() {
    BaseAddress = new Uri(imageStoreUrl)
};

using HttpResponseMessage wpResp = await generalClient.GetAsync(waypointsUrl);
if (!wpResp.IsSuccessStatusCode) {
    throw new Exception($"Failed to fetch waypoints from GitHub: {wpResp.StatusCode}, {wpResp.ReasonPhrase}\n{await wpResp.Content.ReadAsStringAsync()}");
}
List<WaypointDownloadData> waypoints = await wpResp.Content.ReadFromJsonAsync<List<WaypointDownloadData>>() ?? [];

using NpgsqlConnection connection = new(postgresConnectionString);
await connection.OpenAsync();

int mapId = await HandleMap();
Console.WriteLine($"Getting waypoints from {waypointsUrl}");
Console.WriteLine($"Found {waypoints.Count} waypoints to process");
foreach (WaypointDownloadData waypoint in waypoints)
{
    Console.WriteLine($"Processing waypoint: {waypoint.Name}");
    await HandleWaypoint(waypoint.DownloadUrl, mapId);
}

Console.WriteLine($"Loading {waypoints.Count} waypoints");

// **************
// Helper methods
//***************

async Task<int> HandleMap()
{
    Console.WriteLine($"Downloading map from {mapDataUrl}");
    (string imageId, Stream fileStream)? imageResp = await HandleImage(mapDataUrl);
    if (imageResp == null) {
        Console.WriteLine($"Skipping map due to image download failure: {mapDataUrl}");
        throw new Exception("Failed to download map image");
    }

    Console.WriteLine($"Inserting map with image ID {imageResp.Value.imageId} into database");
    using NpgsqlCommand command = new(
        "INSERT INTO map (image_id, ne_longitude, ne_latitude, sw_longitude, sw_latitude) " +
        "VALUES (@ImageId, 12.144353, 55.655294, 12.134139, 55.651107) RETURNING id",
        connection);
    command.Parameters.AddWithValue("ImageId", imageResp.Value.imageId);

    object mapId = await command.ExecuteScalarAsync()
        ?? throw new Exception("Failed to insert map into database");

    Console.WriteLine($"Map inserted with ID {(int)mapId}");
    return (int)mapId;
}

async Task HandleWaypoint(string downloadUrl, int mapId)
{
    Console.WriteLine($"\tDownloading waypoint image from {downloadUrl}");
    (string imageId, Stream fileStream)? imageResp = await HandleImage(downloadUrl);
    if (imageResp == null)
    {
        Console.WriteLine($"Skipping waypoint due to image download failure: {downloadUrl}");
        return;
    }

    WaypointMetadata metadata = await ReadImageMetadata(imageResp.Value.fileStream);

    Console.WriteLine($"\tInserting waypoint with image ID {imageResp.Value.imageId}, " +
                      $"lat: {metadata.Latitude}, lng: {metadata.Longitude}, height: {metadata.Height} into database");
    using NpgsqlCommand command = new(
        "INSERT INTO waypoint (image_id, latitude, longitude, height, map_id) VALUES (@ImageId, @Latitude, @Longitude, @Height, @MapId)",
        connection);
    command.Parameters.AddWithValue("ImageId", imageResp.Value.imageId);
    command.Parameters.AddWithValue("Latitude", metadata.Latitude);
    command.Parameters.AddWithValue("Longitude", metadata.Longitude);
    command.Parameters.AddWithValue("Height", metadata.Height);
    command.Parameters.AddWithValue("MapId", mapId);

    await command.ExecuteNonQueryAsync();
}

async Task<(string id, Stream fileStream)?> HandleImage(string downloadUrl)
{
    using HttpResponseMessage imageResponse = await generalClient.GetAsync(downloadUrl);
    if (!imageResponse.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to download {downloadUrl}: {imageResponse.StatusCode}");
        return null;
    }

    using Stream imageData = await imageResponse.Content.ReadAsStreamAsync();
    using StreamContent fileContent = new(imageData);
    using MultipartFormDataContent content = new() {
        { fileContent, "file", $"placeholder.jpg" }
    };
    using HttpResponseMessage response = await imageStoreClient.PostAsync("image-api/images", content);
    if (!response.IsSuccessStatusCode)
    {
        Console.WriteLine($"Failed to upload image: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");
        return null;
    }

    ImageApiPostResponse? resp = await response.Content.ReadFromJsonAsync<ImageApiPostResponse>();
    if (resp == null)
    {
        Console.WriteLine("Failed to parse response from image API");
        return null;
    }

    imageData.Seek(0, SeekOrigin.Begin);
    MemoryStream metadataStream = new();
    imageData.CopyTo(metadataStream);
    metadataStream.Seek(0, SeekOrigin.Begin);
    return (resp.Id, metadataStream);
}

async Task<WaypointMetadata> ReadImageMetadata(Stream imageStream)
{
    ImageFile img = await ImageFile.FromStreamAsync(imageStream);
    GPSLatitudeLongitude lat = img.Properties.Get<GPSLatitudeLongitude>(ExifTag.GPSLatitude);
    GPSLatitudeLongitude lng = img.Properties.Get<GPSLatitudeLongitude>(ExifTag.GPSLongitude);
    ExifURational height = img.Properties.Get<ExifURational>(ExifTag.GPSAltitude);

    return new WaypointMetadata()
    {
        Latitude = lat.ToFloat(),
        Longitude = lng.ToFloat(),
        Height = height.ToFloat(),
    };
}

public class WaypointDownloadData
{
    public required string Name { get; set; }
    [JsonPropertyName("download_url")]
    public required string DownloadUrl { get; set; }
}

public class ImageApiPostResponse
{
    public required string Url { get; set; }
    public required string Id { get; set; }
}

public class WaypointMetadata
{
    public required float Latitude { get; set; }
    public required float Longitude { get; set; }
    public required float Height { get; set; }
}
