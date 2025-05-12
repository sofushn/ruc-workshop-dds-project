using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace ImageStoreAPI;

public class SyncBackgroundService : BackgroundService
{
    private readonly ILogger<SyncBackgroundService> _logger;
    private readonly string _imagesDirectory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IOptionsMonitor<ReplicationOptions> _replicationOptions;

    private static ConcurrentDictionary<string, bool> _syncInProgress = new();

    public SyncBackgroundService(
        ILogger<SyncBackgroundService> logger,
        IWebHostEnvironment environment,
        IOptionsMonitor<ReplicationOptions> replicationOptions,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _imagesDirectory = Utils.GetImageFolderPath(environment);


        _httpClientFactory = httpClientFactory;
        _replicationOptions = replicationOptions;
    }

    protected async override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        ReplicationOptions staticOptions = _replicationOptions.CurrentValue;
        if (!staticOptions.Enabled) {
            _logger.LogInformation("Replication is disabled.");
            return;
        } else if (!staticOptions.IsPrimary) {
            _logger.LogInformation("Sync service disabled for replica instances.");
            return;
        }

        _logger.LogInformation("Sync service running in the background.");

        using PeriodicTimer timer = new(TimeSpan.FromSeconds(staticOptions.SyncIntervalSeconds));
        try {
            while (await timer.WaitForNextTickAsync(stoppingToken)) {
                IEnumerable<string> ids = Directory.EnumerateFiles(_imagesDirectory)
                    .Select(Path.GetFileNameWithoutExtension)!;
                await Parallel.ForEachAsync(_replicationOptions.CurrentValue.ReplicaUrls, async (url, cancellationToken) => await Sync(url, ids));
            }
        } catch (OperationCanceledException) {
            _logger.LogInformation("Timed Hosted Service is stopping.");
        }
    }

    private async Task Sync(string url, IEnumerable<string> ids)
    {
        if (_syncInProgress.GetOrAdd(url, false)) {
            _logger.LogDebug($"Sync already in progress for {url}");
            return;
        }

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
        _syncInProgress.AddOrUpdate(url, false, (key, value) => false);
    }
}
