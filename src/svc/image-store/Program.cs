using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
}

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "Images")),
    RequestPath = "/images"
});

var image = app.MapGroup("/image-api");

image.MapGet("images", async (HttpContext context) =>
{
    var imagesPath = Path.Combine(builder.Environment.ContentRootPath, "Images");

    if(!Directory.EnumerateFiles(imagesPath).Any())
    {
        return Results.NotFound("No images found.");
    }

    var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}{context.Request.Path}";

    var imageFiles = await Task.Run(() => Directory.EnumerateFiles(imagesPath)
                                  .Select(Path.GetFileName)
                                  .Select(fileName => $"{baseUrl}/{fileName}"));

    return Results.Ok(imageFiles);
})
.WithName("GetImageLinks");

image.MapGet("images/{fileName}", (string fileName) =>
{
    var filePath = Path.Combine(builder.Environment.ContentRootPath, "Images", fileName);

    if (!File.Exists(filePath))
    {
        return Results.NotFound("Image not found.");
    }

    return Results.File(filePath, "image/jpeg");
})
.WithName("GetImage");

image.MapPost("images", async (IFormFile? file, string? fileName, HttpRequest request) =>
{
    if(string.IsNullOrEmpty(fileName))
    {
        return Results.BadRequest("File name is required. Please try again!");
    }

    if(file == null || file.Length == 0)
    {
        return Results.BadRequest("File is required. Please try again!");
    }

    var imagesPath = Path.Combine(builder.Environment.ContentRootPath, "Images");

    var filePath = Path.Combine(imagesPath, fileName+".jpg");
    
    using var stream = File.OpenWrite(filePath);
    await file.CopyToAsync(stream);

    var imgUrl = $"{request.Scheme}://{request.Host}{request.Path}/{fileName}.jpg";

    return Results.Created(imgUrl, new { Url = imgUrl });
})
.WithName("PostImage")
.DisableAntiforgery();

app.Run();

