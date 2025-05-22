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
        => ValueTask.FromResult<WaypointPostRequest?>(new() {
            Latitude = decimal.Parse(httpContext.Request.Form["latitude"]),
            Longitude = decimal.Parse(httpContext.Request.Form["longitude"]),
            Height = decimal.Parse(httpContext.Request.Form["height"]),
            MapId = int.Parse(httpContext.Request.Form["mapId"]),
            File = httpContext.Request.Form.Files[0]
        }
        );
}

public class ImageApiPostResponse
{
    public required string Url { get; set; }
    public required Guid Id { get; set; }
}
