using System.Collections.Concurrent;
using Microsoft.Extensions.Options;

namespace ImageStoreAPI;

public class SyncBackgroundService : BackgroundService
{
    private readonly ILogger<SyncBackgroundService> _logger;
    private readonly string _imagesDirectory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ReplicationOptions _replicationOptions;

    private static Dictionary<string, BlockingCollection<string>> _filesBeingSynced;

    public SyncBackgroundService(
        ILogger<SyncBackgroundService> logger,
        IWebHostEnvironment environment,
        IOptionsMonitor<ReplicationOptions> replicationOptions,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _imagesDirectory = Utils.GetImageFolderPath(environment);

        _filesBeingSynced = new Dictionary<string, BlockingCollection<string>>();
        foreach (string url in replicationOptions.CurrentValue.ReplicaUrls) {
            _filesBeingSynced.TryAdd(url, []);
        }

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
        
        foreach (string id in missingIds) {
            if (!_filesBeingSynced[url].Contains(id))
                _filesBeingSynced[url].Add(id);
        }

        _logger.LogInformation($"Syncing {missingIds.Count} files to replica: {url}");

        for (int i = 0; i < missingIds.Count; i++) {
            string id = _filesBeingSynced[url].Take();
            try {
                string filePath = Path.Combine(_imagesDirectory, $"{id}.jpg");

                using var stream = File.OpenRead(filePath);
                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(stream);
                content.Add(fileContent, "file", $"{id}.jpg");
                content.Add(new StringContent(id), "id");

                HttpResponseMessage syncResponse = await client.PostAsync($"{url}/sync/request", content);
                if (!syncResponse.IsSuccessStatusCode) 
                    _logger.LogWarning($"Failed to sync file ({id}) to replica: {url}");
            } catch {
                _filesBeingSynced[url].Add(id);
                throw;
            }
        }
    }
}
