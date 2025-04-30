using Npgsql;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<Context>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresDatabase")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(options =>
    {
        options.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
}

app.UseHttpsRedirection();

app.MapGet("/map/{mapId}/coordinates", CoordinatesHandler.GetAll);
app.MapGet("/coordinates/{coordinateId}", CoordinatesHandler.GetById);
app.MapPost("/coordinates/{coordinateId}", CoordinatesHandler.Create);
app.MapPut("/coordinates/{coordinateId}", CoordinatesHandler.Update);
app.MapDelete("/coordinates/{coordinateId}", CoordinatesHandler.Delete);

app.MapGet("/map", MapHandler.GetAll);
app.MapGet("/map/{mapId}", MapHandler.GetById);

app.Run();
