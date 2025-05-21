using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using static System.Net.Mime.MediaTypeNames;

namespace Api;

public class CoordinatesHandler {

    private readonly IHttpClientFactory _httpClientFactory;

    public CoordinatesHandler(IHttpClientFactory httpClientFactory) =>
        _httpClientFactory = httpClientFactory;

    public static IResult GetAll(int mapId, [FromServices] MetadataContext context)
        => Results.Ok(context.GPSCoordinates.Where(x => x.MapId == mapId).AsNoTracking());
    
    public static IResult GetById(int coordinateId, [FromServices] MetadataContext context)
    {
        var coord = context.GPSCoordinates.FirstOrDefault(x => x.Id == coordinateId);
        return coord is not null ? Results.Ok(coord) : Results.NotFound();
    }

    public async Task<IResult> Create(GPSCoordinate coordinate, [FromServices] MetadataContext context, IFormFile image )
    {
        /*
        var httpRequestMessage = new HttpRequestMessage(
            HttpMethod.Post,
            "http//:image-api/images"
        )
        {
            Headers =
            {HeaderNames.Accept, "images"}
        };
        */

        var httpClient = _httpClientFactory.CreateClient();

        var imageJson = new StringContent(
            JsonSerializer.Serialize(image),
            Encoding.UTF8,
            Application.Json
        );

        using var HttpResponseMessage =
            await httpClient.PostAsync("image-api/images", imageJson);

        HttpResponseMessage.EnsureSuccessStatusCode();

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