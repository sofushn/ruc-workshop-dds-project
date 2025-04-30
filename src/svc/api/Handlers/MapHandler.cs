using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api;

public class MapHandler
{

    public static IResult GetAll([FromServices] Context.Context context)
      => Results.Ok(context.Maps.AsNoTracking());
    public static IResult GetById(int mapId, [FromServices] Context.Context context)
    {
        var map = context.Maps.FirstOrDefault(m => m.Id == mapId);
        return map != null ? Results.Ok(map) : Results.NotFound();
    } 
}
