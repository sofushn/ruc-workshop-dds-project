using Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MetadataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDatabase")));



builder.Services.AddHttpClient("image-api", client =>
{
    string imageStoreUrl = builder.Configuration.GetValue<string>("ImageStoreUrl")
        ?? throw new Exception("ImageStoreUrl is not configured.");
    client.BaseAddress = new Uri(imageStoreUrl);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseCors(options =>
    {
        options.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
}

app.UseHttpsRedirection();

RouteGroupBuilder apiGroup = app.MapGroup("/api");

apiGroup.MapGet("waypoint/{id}", WaypointHandler.GetById);
apiGroup.MapPost("waypoint/{id}", WaypointHandler.Create)
    .Accepts<WaypointPostRequest>("multipart/form-data")
    .Produces(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest)
    .Produces(StatusCodes.Status500InternalServerError)
    .DisableAntiforgery(); // Disable CSRF protection for this endpoint
apiGroup.MapDelete("waypoint/{id}", WaypointHandler.Delete);
apiGroup.MapPut("waypoint/{id}", WaypointHandler.Update);

apiGroup.MapGet("map/{id}/waypoints", WaypointHandler.GetAll);
apiGroup.MapGet("map", MapHandler.GetAll);
apiGroup.MapGet("map/{id}", MapHandler.GetById);

app.Run();
