using Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

builder.Services.AddSingleton<CoordinateStore>();

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

app.MapGet("/coordinates", CoordinatesHandler.GetAll);
app.MapGet("/coordinates/{imageId}", CoordinatesHandler.GetById);
app.MapPost("/coordinates", CoordinatesHandler.Create);
app.MapPut("/coordinates/{imageId}", CoordinatesHandler.Update);
app.MapDelete("/coordinates/{imageId}", CoordinatesHandler.Delete);

app.Run();
