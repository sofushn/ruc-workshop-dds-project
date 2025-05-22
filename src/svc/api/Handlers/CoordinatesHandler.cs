using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static System.Net.Mime.MediaTypeNames;

namespace Api;

public class CoordinatesHandler {
    public static IResult GetAll(int mapId, [FromServices] MetadataContext context)
        => Results.Ok(context.GPSCoordinates.Where(x => x.MapId == mapId).AsNoTracking());
    
    public static IResult GetById(int coordinateId, [FromServices] MetadataContext context)
    {
        var coord = context.GPSCoordinates.FirstOrDefault(x => x.Id == coordinateId);
        return coord is not null ? Results.Ok(coord) : Results.NotFound();
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

        GPSCoordinate coordinate = new() {
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ImageId = (postResp?.Id ?? Guid.Empty).ToString(),
            Height = request.Height,
            MapId = request.MapId
        };

        Console.WriteLine($"Coordinate: {JsonSerializer.Serialize(coordinate)}");

        context.GPSCoordinates.Add(coordinate);
        context.SaveChanges();
        return Results.Created($"/coordinates/{coordinate.ImageId}", coordinate);
    }

    public static IResult Update(int coordinateId, GPSCoordinate updated, [FromServices] MetadataContext context)
    {
        var coord = context.GPSCoordinates.FirstOrDefault(x => x.Id == coordinateId);
        if (coord == null) return Results.NotFound();

        context.Entry(coord).CurrentValues.SetValues(updated);
        context.SaveChanges();

        return Results.Ok(updated);    
    }
    public static IResult Delete(int coordinateId, [FromServices] MetadataContext context)
    {
        var coord = context.GPSCoordinates.FirstOrDefault(x => x.Id == coordinateId);
        if (coord == null) return Results.NotFound();

        context.GPSCoordinates.Remove(coord);
        return Results.Ok(); 
    }

}