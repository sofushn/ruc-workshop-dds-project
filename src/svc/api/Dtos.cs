using System.Reflection;

namespace Api;

public class WaypointPostRequest
{
    public required IFormFile File { get; set; }
    public required decimal Latitude { get; set; }
    public required decimal Longitude { get; set; }
    public required decimal Height { get; set; }
    public required int MapId { get; set; }

    public static ValueTask<WaypointPostRequest?> BindAsync(HttpContext httpContext, ParameterInfo parameter)
    {
        if (httpContext.Request.Form.Files.Count == 0)
            return ValueTask.FromResult<WaypointPostRequest?>(null);

        if (!decimal.TryParse(httpContext.Request.Form["latitude"], out decimal latitude) ||
            !decimal.TryParse(httpContext.Request.Form["longitude"], out decimal longitude) ||
            !decimal.TryParse(httpContext.Request.Form["height"], out decimal height) ||
            !int.TryParse(httpContext.Request.Form["mapId"], out int mapId))
        {
            return ValueTask.FromResult<WaypointPostRequest?>(null);
        }

        return ValueTask.FromResult<WaypointPostRequest?>(new() {
            Latitude = latitude,
            Longitude = longitude,
            Height = height,
            MapId = mapId,
            File = httpContext.Request.Form.Files[0]
        });
    }
}

public class ImageApiPostResponse
{
    public required string Url { get; set; }
    public required Guid Id { get; set; }
}
