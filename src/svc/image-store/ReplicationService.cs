using Microsoft.Extensions.Options;

namespace ImageStoreAPI;

public class ReplicationService {
    private ILogger<ReplicationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsSnapshot<ReplicationOptions> _options;

    public ReplicationService(
        IOptionsSnapshot<ReplicationOptions> options, 
        ILogger<ReplicationService> logger, 
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
        _options = options;
    }

    public async Task SyncFile(Guid id, IFormFile file) {
        if (!_options.Value.IsPrimary || !_options.Value.Enabled)
            return;
        
        await Parallel.ForEachAsync(_options.Value.ReplicaUrls, async (url, cancellationToken) => {
            try {
                await SyncFileToReplica(url, id, file);
            } catch (Exception ex) {
                _logger.LogWarning(ex, $"Failed to sync file to replica: {url}");
            }
        });
    }

    private async Task SyncFileToReplica(string url, Guid id, IFormFile file) {
        using var client = _httpClientFactory.CreateClient();
        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        using var fileContent = new StreamContent(stream);
        
        content.Add(fileContent, "file", file.FileName);
        content.Add(new StringContent(id.ToString()), "id");
        
        HttpResponseMessage response = await client.PostAsync($"{url}/sync/request", content);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to sync file to replica: {url}");
    }
}