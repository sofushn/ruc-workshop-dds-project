using Npgsql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MetadataContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDatabase")));


builder.Services.AddHttpClient();

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

apiGroup.MapGet("coordinates/{coordinateId}", CoordinatesHandler.GetById);
apiGroup.MapPost("coordinates/{coordinateId}", CoordinatesHandler.Create);
apiGroup.MapDelete("coordinates/{coordinateId}", CoordinatesHandler.Delete);
apiGroup.MapPut("coordinates/{coordinateId}", CoordinatesHandler.Update);

apiGroup.MapGet("map/{mapId}/coordinates", CoordinatesHandler.GetAll);
apiGroup.MapGet("map", MapHandler.GetAll);
apiGroup.MapGet("map/{mapId}", MapHandler.GetById);

app.Run();
