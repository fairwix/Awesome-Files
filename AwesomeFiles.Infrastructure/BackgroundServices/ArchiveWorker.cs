using AwesomeFiles.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AwesomeFiles.Infrastructure.BackgroundServices;
public class ArchiveWorker : BackgroundService
{
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<ArchiveWorker> _logger;

    public ArchiveWorker(IBackgroundTaskQueue taskQueue, ILogger<ArchiveWorker> logger)
    {
        _taskQueue = taskQueue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ArchiveWorker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var workItem = await _taskQueue.DequeueAsync(stoppingToken);
                await workItem(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("ArchiveWorker stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing work item");
            }
        }
    }
}