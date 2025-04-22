using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Middleware to use static files, in our case img from folder "Images"
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "Images")),
    RequestPath = "/images"
});

//Gets lists of links to all images
app.MapGet("/images", async (HttpContext context) =>
{
    //Select path for images
    var imagesPath = Path.Combine(builder.Environment.ContentRootPath, "Images");

    //URL based on current Request. Aka dynamically choose whether request is HTTP/HTTPS and host number (including port i guess)
    var baseUrl = $"{context.Request.Scheme}://{context.Request.Host}/images";

    //Dunno if await Task.Run is best to handle async, other ideas?
    var imageFiles = await Task.Run(() => Directory.EnumerateFiles(imagesPath)
                                  .Select(Path.GetFileName)
                                  .Select(fileName => $"{baseUrl}/{fileName}"));

    return Results.Ok(imageFiles);
})
.WithName("GetImageLinks")
.WithOpenApi();

//Redirects to errors/STATUSCODE, eg. 404, 501 etc..
app.UseStatusCodePagesWithRedirects("/errors/{0}");

// Custom 404 error page
app.MapGet("/errors/404", async (HttpContext context) =>
{

    using HttpClient client = new();
    try
    {
        //Fetching img to display
        HttpResponseMessage response = await client.GetAsync("https://http.dog/404.jpg");

        //This throws error if not OK response
        response.EnsureSuccessStatusCode();

        //Stores the binary img data in byte array
        byte[] imageData = await response.Content.ReadAsByteArrayAsync();

        //Setting response type, based on type from original response
        context.Response.ContentType = response.Content.Headers.ContentType.ToString();

        //Sends img data back to client
        await context.Response.Body.WriteAsync(imageData, 0, imageData.Length);
    }
    catch (HttpRequestException)
    {
        //In progress.....
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Dummy text, not good to get error when trying to display 404 error img :)");
    }
});

app.Run();
