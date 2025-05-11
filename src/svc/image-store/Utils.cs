namespace ImageStoreAPI;

public static class Utils
{
    public static string GetImageFolderPath(IWebHostEnvironment environment)
        => environment.EnvironmentName == "Development"
            ? Path.Combine(environment.ContentRootPath, "Images")
            : "/images";
    
    public static void WriteToDisk(IWebHostEnvironment environment, string fileName, IFormFile file) {
        string filePath = Path.Combine(GetImageFolderPath(environment), $"{fileName}.jpg");
        using var stream = File.OpenWrite(filePath);
        file.CopyTo(stream);
    }
}