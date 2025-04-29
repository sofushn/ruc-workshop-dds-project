
using Api.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Handlers;

public class MapHandlers
{

    public static IResult GetById(int mapId, [FromServices] Context.Context context)
    {
        var map = context.Maps.FirstOrDefault(m => m.Id == mapId);
        return map != null ? Results.Ok(map) : Results.NotFound();
    } 
}
