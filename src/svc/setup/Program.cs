using System.Net.Http.Json;
using ExifLibrary;
using Npgsql;

string waypointsUrl = "https://api.github.com/repos/sofushn/ruc-workshop-dds-project/contents/images/waypoints";
string mapDataUrl = "https://raw.githubusercontent.com/sofushn/ruc-workshop-dds-project/main/images/maps/2dmap.jpg";

string ghToken = Environment.GetEnvironmentVariable("GITHUB_TOKEN") 
    ?? throw new ArgumentNullException("GITHUB_TOKEN environment variable is not set");

string postgresConnectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING") 
    ?? throw new ArgumentNullException("POSTGRES_CONNECTION_STRING environment variable is not set");

string imageStoreUrl = Environment.GetEnvironmentVariable("IMAGE_STORE_URL") 
    ?? throw new ArgumentNullException("IMAGE_STORE_URL environment variable is not set");

HttpClient generalClient = new();
using HttpResponseMessage mapResponse = await generalClient.GetAsync(mapDataUrl);

HttpClient ghApiClient = new() 
{
    DefaultRequestHeaders = {
        Authorization = new("Bearer", ghToken)
    },
};
List<WaypointDownloadData> waypoints = await ghApiClient.GetFromJsonAsync<List<WaypointDownloadData>>(waypointsUrl) 
    ?? new List<WaypointDownloadData>();

HttpClient imageStoreClient = new()
{
    BaseAddress = new Uri(imageStoreUrl)
};

using NpgsqlConnection connection = new(postgresConnectionString);
await connection.OpenAsync();

int mapId = await HandleMap();
foreach (WaypointDownloadData waypoint in waypoints)
{
    await HandleWaypoint(waypoint.DownloadUrl, mapId);
}

Console.WriteLine($"Loading {waypoints.Count} waypoints");

async Task<int> HandleMap()
{
    (string imageId, Stream fileStream)? imageResp = await HandleImage(mapDataUrl);
    if (imageResp == null)
    {
        Console.WriteLine($"Skipping map due to image download failure: {mapDataUrl}");
        throw new Exception("Failed to download map image");
    }

    using NpgsqlCommand command = new(
        "INSERT INTO map (image_id, ne_longitude, ne_latitude, sw_longitude, sw_latitude) " +
        "VALUES (@ImageId, 12.144353, 55.655294, 12.134139, 55.651107) RETURNING id",
        connection);
    command.Parameters.AddWithValue("ImageId", imageResp.Value.imageId);

    object mapId = await command.ExecuteScalarAsync()
        ?? throw new Exception("Failed to insert map into database");

    return (int)mapId;
}
;
async Task HandleWaypoint(string downloadUrl, int mapId)
{
    (string imageId, Stream fileStream)? imageResp = await HandleImage(downloadUrl);
    if (imageResp == null)
    {
        Console.WriteLine($"Skipping waypoint due to image download failure: {downloadUrl}");
        return;
    }

    WaypointMetadata metadata = await ReadImageMetadata(imageResp.Value.fileStream);

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
        Latitude = Convert.ToDecimal(lat.ToFloat()),
        Longitude = Convert.ToDecimal(lng.ToFloat()),
        Height = Convert.ToDecimal(height.ToFloat()),
    };
}

public class WaypointDownloadData
{
    public required string Name { get; set; }
    public required string DownloadUrl { get; set; }
}

public class ImageApiPostResponse
{
    public required string Url { get; set; }
    public required string Id { get; set; }
}

public class WaypointMetadata
{
    public required decimal Latitude { get; set; }
    public required decimal Longitude { get; set; }
    public required decimal Height { get; set; }
}
