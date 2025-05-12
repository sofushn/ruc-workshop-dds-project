using Microsoft.Extensions.Options;

namespace ImageStoreAPI;

public class ReplicationService {
    private ILogger<ReplicationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _environment;
    private readonly IOptionsSnapshot<ReplicationOptions> _options;

    public ReplicationService(
        IOptionsSnapshot<ReplicationOptions> options, 
        ILogger<ReplicationService> logger, 
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment environment)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _environment = environment;
        _options = options;
    }

    public async Task SyncFileAsync(Guid id, string fileId) {
        if (!_options.Value.IsPrimary || !_options.Value.Enabled)
            return;
        
        await Parallel.ForEachAsync(_options.Value.ReplicaUrls, async (url, cancellationToken) => {
            try {
                await SyncFileToReplicaAsync(url, fileId);
            } catch (Exception ex) {
                _logger.LogWarning(ex, $"Failed to sync file to replica: {url}");
            }
        });
    }

    private async Task SyncFileToReplicaAsync(string url, string fileId) {
        string filePath = Path.Combine(Utils.GetTempFolderPath(_environment), $"{fileId}.jpg");
        
        using FileStream stream = File.OpenRead(filePath);
        using StreamContent fileContent = new(stream);
        
        using MultipartFormDataContent content = new() {
            { fileContent, "file", $"{fileId}.jpg" },
            { new StringContent(fileId), "id" }
        };
        
        using HttpClient client = _httpClientFactory.CreateClient();
        HttpResponseMessage response = await client.PostAsync($"{url}/sync/request", content);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to sync file to replica: {url}");
    }
}
