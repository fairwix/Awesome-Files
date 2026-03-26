namespace AwesomeFiles.Client.Application;

public interface IArchiveClientService
{
    Task CreateAndWaitAndDownloadAsync(string[] fileNames, string destinationFolder, CancellationToken cancellationToken = default);
    
    Task WaitForCompletionAsync(Guid taskId, CancellationToken cancellationToken = default);
}