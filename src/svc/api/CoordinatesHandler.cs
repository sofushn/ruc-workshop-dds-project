using Microsoft.AspNetCore.Mvc;

namespace Api;

public class CoordinatesHandler {
    public static IResult GetAll([FromServices] CoordinateStore store)
        => Results.Ok(store.GetAll());
    
    public static IResult GetById(string imageId, [FromServices] CoordinateStore store)
    {
        var coord = store.Get(imageId);
        return coord is not null ? Results.Ok(coord) : Results.NotFound();
    }

    public static IResult Create(GPSCoordinate coordinate, [FromServices] CoordinateStore store)
    {
        store.Add(coordinate);
        return Results.Created($"/coordinates/{coordinate.ImageId}", coordinate);
    }

    public static IResult Update(string imageId, GPSCoordinate updated, [FromServices] CoordinateStore store)
    {
        if (!store.Update(imageId, updated))
            return Results.NotFound();

        return Results.Ok(updated);    
    }
    public static IResult Delete(String imageId, [FromServices] CoordinateStore store)
    {
        return store.Delete(imageId) ? Results.Ok() : Results.NotFound();
    }
}