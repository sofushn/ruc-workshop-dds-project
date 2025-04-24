using Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<CoordinateStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/coordinates", CoordinatesHandler.GetAll);
app.MapGet("/coordinates/{imageId}", CoordinatesHandler.GetById);
app.MapPost("/coordinates", CoordinatesHandler.Create);
app.MapPut("/coordinates/{imageId}", CoordinatesHandler.Update);
app.MapDelete("/coordinates/{imageId}", CoordinatesHandler.Delete);

// Add dummy data to the in-memory store, remove later.
var coordinateStore = app.Services.GetRequiredService<CoordinateStore>();

coordinateStore.Add(new GPSCoordinate
{
    ImageId = "image-01",
    X = 12.34,
    Y = 56.78
});

coordinateStore.Add(new GPSCoordinate
{
    ImageId = "image-02",
    X = 98.76,
    Y = 54.32
});

coordinateStore.Add(new GPSCoordinate
{
    ImageId = "image-03",
    X = 66.81,
    Y = 32.44
});

coordinateStore.Add(new GPSCoordinate
{
    ImageId = "image-04",
    X = 58.06,
    Y = 74.28
});

app.Run();
