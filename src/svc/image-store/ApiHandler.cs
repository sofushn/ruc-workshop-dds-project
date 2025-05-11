using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using System.ComponentModel.DataAnnotations;

namespace ImageStoreAPI;

public static class ApiHandler {
    public static IResult GetAll([FromServices] IWebHostEnvironment environment, HttpRequest request) {
        string imagesPath = Utils.GetImageFolderPath(environment);

        IEnumerable<string> files = Directory.EnumerateFiles(imagesPath)
            .Select(Path.GetFileName)
            .Select(fileName => $"{request.GetEncodedUrl()}/{fileName}");
        
        return files.Any()
            ? Results.Ok(files)
            : Results.Ok(Enumerable.Empty<string>());
    }

    public static IResult Get([FromServices] IWebHostEnvironment environment, string fileName) {
        if (!Path.HasExtension(fileName))
            fileName += ".jpg";
        
        string filePath = Path.Combine(Utils.GetImageFolderPath(environment), fileName);

        return File.Exists(filePath)
            ? Results.File(filePath, "image/jpeg")
            : Results.NotFound();
    }

    public static IResult Post([FromServices] IWebHostEnvironment environment, [Required] IFormFile file, HttpRequest request) {
        Guid fileName = Guid.NewGuid();
        string filePath = Path.Combine(environment.ContentRootPath, "Images", $"{fileName}.jpg");
        
        using var stream = File.OpenWrite(filePath);
        file.CopyTo(stream);

        string imgUrl = $"{request.GetEncodedUrl()}/{fileName}.jpg";

        return Results.Created(imgUrl, new { Url = imgUrl });
    }
}
