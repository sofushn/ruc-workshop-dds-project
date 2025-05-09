using ImageStoreAPI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment()) {
    app.UseCors(policy => policy
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader()
    );
}

app.UseHttpsRedirection();

var image = app.MapGroup("/image-api");

image.MapGet("images", ApiHandler.GetAll)
    .WithName("GetImageLinks")
    .Produces<string[]>(StatusCodes.Status200OK);

image.MapGet("images/{fileName}", ApiHandler.Get)
    .WithName("GetImage")
    .Produces(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status404NotFound);

image.MapPost("images", ApiHandler.Post)
    .DisableAntiforgery() // Disable CSRF protection for this endpoint
    .WithName("PostImage")
    .Accepts<IFormFile>("multipart/form-data")
    .Produces(StatusCodes.Status201Created)
    .Produces(StatusCodes.Status400BadRequest);

app.Run();
