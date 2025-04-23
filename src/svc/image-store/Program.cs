using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "Images")),
    RequestPath = "/images"
});

app.MapGet("/images", async (HttpContext context) =>
{
    var imagesPath = Path.Combine(builder.Environment.ContentRootPath, "Images");

    var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}/images";

    var imageFiles = await Task.Run(() => Directory.EnumerateFiles(imagesPath)
                                  .Select(Path.GetFileName)
                                  .Select(fileName => $"{baseUrl}/{fileName}"));

    return Results.Ok(imageFiles);
})
.WithName("GetImageLinks")
.WithOpenApi();

app.UseStatusCodePagesWithRedirects("/errors/{0}");

app.MapGet("/errors/404", async (HttpContext context) =>
{

    using HttpClient client = new();
    try
    {
        HttpResponseMessage response = await client.GetAsync("https://http.dog/404.jpg");

        response.EnsureSuccessStatusCode();

        byte[] imageData = await response.Content.ReadAsByteArrayAsync();

        context.Response.ContentType = response.Content.Headers.ContentType.ToString();

        await context.Response.Body.WriteAsync(imageData, 0, imageData.Length);
    }
    catch (HttpRequestException)
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Dummy text, not good to get error when trying to display 404 error img :)");
    }
});

app.Run();
