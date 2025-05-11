using Microsoft.Extensions.Options;

namespace ImageStoreAPI;

public class SyncBackgroundService : BackgroundService
{
    private readonly ILogger<SyncBackgroundService> _logger;
    private readonly string _imagesDirectory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ReplicationOptions _replicationOptions;

    public SyncBackgroundService(
        ILogger<SyncBackgroundService> logger,
        IWebHostEnvironment environment,
        IOptionsMonitor<ReplicationOptions> replicationOptions,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _imagesDirectory = Utils.GetImageFolderPath(environment);
        _httpClientFactory = httpClientFactory;
        _replicationOptions = replicationOptions.CurrentValue;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_replicationOptions.Enabled) {
            _logger.LogInformation("Replication is disabled.");
            return;
        } else if (!_replicationOptions.IsPrimary) {
            _logger.LogInformation("Sync service disabled for replica instances.");
            return;
        }

        _logger.LogInformation("Sync service running in the background.");

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(_replicationOptions.SyncIntervalSeconds));
        try {
            while (await timer.WaitForNextTickAsync(stoppingToken)) {
                IEnumerable<string> ids = Directory.EnumerateFiles(_imagesDirectory)
                    .Select(Path.GetFileNameWithoutExtension)!;
                Parallel.ForEach(_replicationOptions.ReplicaUrls, async url => await Sync(url, ids));
            }
        } catch (OperationCanceledException) {
            _logger.LogInformation("Timed Hosted Service is stopping.");
        }
    }

    private async Task Sync(string url, IEnumerable<string> ids)
    {
        HttpClient client = _httpClientFactory.CreateClient();
        HttpResponseMessage response = await client.PostAsJsonAsync($"{url}/sync/check", ids);

        if (!response.IsSuccessStatusCode) {
            _logger.LogWarning($"Failed to check for missing files on replica: {url}");
            return;
        }

        List<string> missingIds = await response.Content.ReadFromJsonAsync<List<string>>() ?? [];
        if(!missingIds.Any())
            return;

        _logger.LogInformation($"Syncing {missingIds.Count} files to replica: {url}");

        foreach (string id in missingIds) {
            string filePath = Path.Combine(_imagesDirectory, $"{id}.jpg");

            using var stream = File.OpenRead(filePath);
            using var content = new MultipartFormDataContent();
            using var fileContent = new StreamContent(stream);
            content.Add(fileContent, "file", $"{id}.jpg");
            content.Add(new StringContent(id), "id");

            HttpResponseMessage syncResponse = await client.PostAsync($"{url}/sync/request", content);
            if (!syncResponse.IsSuccessStatusCode) 
                _logger.LogWarning($"Failed to sync file ({id}) to replica: {url}");
        }
    }
}
