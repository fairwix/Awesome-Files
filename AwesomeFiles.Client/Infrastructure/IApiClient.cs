using AwesomeFiles.Client.Models;

namespace AwesomeFiles.Client.Infrastructure;

public interface IApiClient
{
    Task<string[]> GetFilesAsync(CancellationToken cancellationToken = default);
    Task<CreateArchiveResponse> CreateArchiveAsync(string[] fileNames, CancellationToken cancellationToken = default);
    Task<StatusResponse> GetStatusAsync(Guid taskId, CancellationToken cancellationToken = default);
    Task DownloadArchiveAsync(Guid taskId, string destinationFolder, CancellationToken cancellationToken = default);
}