using System.Reflection;

namespace ImageStoreAPI;

public class ReplicationOptions {
    public bool Enabled { get; set; } = false;
    public bool IsPrimary { get; set; } = false;
    public List<string> ReplicaUrls { get; set; } = new();
    public int SyncIntervalSeconds { get; set; } = 5;
}

public class SyncRequest {
    public required string Id { get; set; }
    public required IFormFile File { get; set; }

    public static ValueTask<SyncRequest?> BindAsync(HttpContext httpContext, ParameterInfo parameter) 
        => ValueTask.FromResult<SyncRequest?>(new() {
                Id = httpContext.Request.Form["id"],
                File = httpContext.Request.Form.Files[0]
            }
        );
}
