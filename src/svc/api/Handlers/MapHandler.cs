using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api;

public class MapHandler
{

    public static IResult GetAll([FromServices] MetadataContext context)
      => Results.Ok(context.Maps.AsNoTracking());
    public static IResult GetById(int id, [FromServices] MetadataContext context)
    {
        var map = context.Maps.FirstOrDefault(m => m.Id == id);
        return map != null ? Results.Ok(map) : Results.NotFound();
    } 
}
