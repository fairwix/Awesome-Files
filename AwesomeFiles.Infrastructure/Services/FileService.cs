using AwesomeFiles.Application.Interfaces.Services;
using AwesomeFiles.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AwesomeFiles.Infrastructure.Services;

public class FileService : IFileService
{
    private readonly string _filesFolder;
    private readonly ILogger<FileService> _logger;

    public FileService(IOptions<FileStorageOptions> options, ILogger<FileService> logger)
    {
        _filesFolder = options.Value.FolderPath;
        _logger = logger;

        if (!Directory.Exists(_filesFolder))
        {
            _logger.LogWarning("Files folder does not exist: {Folder}. Creating...", _filesFolder);
            Directory.CreateDirectory(_filesFolder);
        }
    }

    public Task<string[]> GetAllFilesAsync(CancellationToken cancellationToken = default)
    {
        return Task.Run(() =>
        {
            try
            {
                var files = Directory.GetFiles(_filesFolder)
                    .Select(Path.GetFileName)
                    .Where(f => !string.IsNullOrEmpty(f))
                    .ToArray();
                cancellationToken.ThrowIfCancellationRequested();
                return files!;
            }
            catch (Exception ex)  when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to get files from {Folder}", _filesFolder);
                throw;
            }
        }, cancellationToken);
    }

    public Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(File.Exists(Path.Combine(_filesFolder, fileName)));
    }

    public string GetFullPath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ApplicationException("File name cannot be empty.");

        var fullPath = Path.GetFullPath(Path.Combine(_filesFolder, fileName));
        var rootPath = Path.GetFullPath(_filesFolder);

        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            throw new ApplicationException("Invalid file path.");

        return fullPath;
    }
}