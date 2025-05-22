using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api;

public class WaypointHandler {
    public static IResult GetAll(int id, [FromServices] MetadataContext context)
        => Results.Ok(context.Waypoints.Where(x => x.MapId == id).AsNoTracking());
    
    public static IResult GetById(int id, [FromServices] MetadataContext context)
    {
        var waypoint = context.Waypoints.FirstOrDefault(x => x.Id == id);
        return waypoint is not null ? Results.Ok(waypoint) : Results.NotFound();
    }

    public static async Task<IResult> Create(
        [FromServices] MetadataContext context,
        [FromServices] IHttpClientFactory httpClientFactory,
        WaypointPostRequest request)
    {
        using StreamContent fileContent = new(request.File.OpenReadStream());
        using MultipartFormDataContent content = new() {
            { fileContent, "file", request.File.FileName }
        };

        using HttpClient client = httpClientFactory.CreateClient("image-api");
        using HttpResponseMessage response = await client.PostAsync("image-api/images", content);

        if (!response.IsSuccessStatusCode)
            return Results.Problem($"Failed to upload image: {response.StatusCode}, {await response.Content.ReadAsStringAsync()}");

        ImageApiPostResponse? postResp = await response.Content.ReadFromJsonAsync<ImageApiPostResponse>();

        Waypoint waypoint = new() {
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ImageId = (postResp?.Id ?? Guid.Empty).ToString(),
            Height = request.Height,
            MapId = request.MapId
        };

        context.Waypoints.Add(waypoint);
        context.SaveChanges();
        return Results.Created($"/waypoint/{waypoint.ImageId}", waypoint);
    }

    public static IResult Update(int id, Waypoint updated, [FromServices] MetadataContext context)
    {
        var waypoint = context.Waypoints.FirstOrDefault(x => x.Id == id);
        if (waypoint == null) return Results.NotFound();

        context.Entry(waypoint).CurrentValues.SetValues(updated);
        context.SaveChanges();

        return Results.Ok(updated);    
    }
    public static IResult Delete(int id, [FromServices] MetadataContext context)
    {
        var waypoint = context.Waypoints.FirstOrDefault(x => x.Id == id);
        if (waypoint == null) return Results.NotFound();

        context.Waypoints.Remove(waypoint);
        return Results.Ok();
    }

}