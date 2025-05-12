namespace ImageStoreAPI;

public static class Utils
{
    public static string GetImageFolderPath(IWebHostEnvironment environment)
        => environment.IsDevelopment()
            ? Path.Combine(environment.ContentRootPath, "Images")
            : "/images";
    
    private static string GetTempFolderPath(IWebHostEnvironment environment)
        => environment.IsDevelopment()
            ? Path.Combine(environment.ContentRootPath, "Temp")
            : "/temp";

    public static void WriteToDisk(IWebHostEnvironment environment, string fileName, IFormFile file) {
        string filePath = Path.Combine(GetImageFolderPath(environment), $"{fileName}.jpg");
        using var stream = File.OpenWrite(filePath);
        file.CopyTo(stream);
    }

    public static void WriteToTemp(IWebHostEnvironment environment, string fileName, IFormFile file) {
        string filePath = Path.Combine(GetTempFolderPath(environment), $"{fileName}.jpg");
        using var stream = File.OpenWrite(filePath);
        file.CopyTo(stream);
    }

    public static void MoveFileToPermenentStorage(IWebHostEnvironment environment, string fileName) {
        string tempPath = Path.Combine(GetTempFolderPath(environment), $"{fileName}.jpg");
        string imagePath = Path.Combine(GetImageFolderPath(environment), $"{fileName}.jpg");
        File.Move(tempPath, imagePath);
    }
}