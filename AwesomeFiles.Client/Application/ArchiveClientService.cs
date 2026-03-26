using AwesomeFiles.Client.Infrastructure;
using AwesomeFiles.Client.Models;

namespace AwesomeFiles.Client.Application;

public class ArchiveClientService : IArchiveClientService
{
    private readonly IApiClient _apiClient;

    public ArchiveClientService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task CreateAndWaitAndDownloadAsync(string[] fileNames, string destinationFolder, CancellationToken cancellationToken = default)
    {
        var createResponse = await _apiClient.CreateArchiveAsync(fileNames, cancellationToken);
        var taskId = createResponse.Id;
        Console.WriteLine($"Archive task created, id: {taskId}");
        
        await WaitForCompletionAsync(taskId, cancellationToken);
        
        Console.WriteLine("Archive ready. Downloading...");
        await _apiClient.DownloadArchiveAsync(taskId, destinationFolder, cancellationToken);
        Console.WriteLine($"Archive downloaded to {destinationFolder}");
    }

    public async Task WaitForCompletionAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        StatusResponse? status = null;
        
        do
        {
            await Task.Delay(1000, cancellationToken);
            status = await _apiClient.GetStatusAsync(taskId, cancellationToken);
            Console.WriteLine($"Status: {status.Status}");
            
            if (status.Status == "Failed")
            {
                throw new Exception($"Archive creation failed: {status.Error}");
            }
        } while (status.Status is "Pending" or "InProgress");
    }
}