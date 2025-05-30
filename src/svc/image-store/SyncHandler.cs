using Microsoft.AspNetCore.Mvc;
namespace ImageStoreAPI;

public static class SyncHandler {
    public static async Task<IResult> Request([FromServices] IWebHostEnvironment environment, SyncRequest request) {
        await Utils.WriteToDiskAsync(environment, request.Id, request.File);
        return Results.Ok();
    }

    public static IResult Check([FromServices] IWebHostEnvironment environment, List<string> ids) {
        string imagesPath = Utils.GetImageFolderPath(environment);
        IEnumerable<string> existingIds = Directory.EnumerateFiles(imagesPath).Select(Path.GetFileNameWithoutExtension)!;

        return Results.Ok(ids.Except(existingIds));
    }
}
