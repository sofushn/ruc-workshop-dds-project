using Api.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Handlers;

public class CoordinatesHandler {
    public static IResult GetAll(int mapId, [FromServices] Context.Context context)
        => Results.Ok(context.GPSCoordinates.AsNoTracking());
    
    public static IResult GetById(int coordinateId, [FromServices] Context.Context context)
    {
        var coord = context.GPSCoordinates.FirstOrDefault(x => x.Id == coordinateId);
        return coord is not null ? Results.Ok(coord) : Results.NotFound();
    }

    public static IResult Create(GPSCoordinate coordinate, [FromServices] Context.Context context)
    {
        context.GPSCoordinates.Add(coordinate);
        context.SaveChanges();
        return Results.Created($"/coordinates/{coordinate.ImageId}", coordinate);
    }

    public static IResult Update(int coordinateId, GPSCoordinate updated, [FromServices] Context.Context context)
    {
        var coord = context.GPSCoordinates.FirstOrDefault(x => x.Id == coordinateId);
        if (coord == null) return Results.NotFound();

        context.Entry(coord).CurrentValues.SetValues(updated);
        context.SaveChanges();

        return Results.Ok(updated);    
    }
    public static IResult Delete(int coordinateId, [FromServices] Context.Context context)
    {
        var coord = context.GPSCoordinates.FirstOrDefault(x => x.Id == coordinateId);
        if (coord == null) return Results.NotFound();

        context.GPSCoordinates.Remove(coord);
        return Results.Ok(); 
    }

}