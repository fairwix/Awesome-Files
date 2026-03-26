using AwesomeFiles.Application.Exceptions;
using AwesomeFiles.Application.Interfaces.Services;
using AwesomeFiles.Application.UseCases;
using AwesomeFiles.Domain.Enums;
using Microsoft.Extensions.Logging;
using ApplicationException = AwesomeFiles.Application.Exceptions.ApplicationException;

namespace AwesomeFiles.Application.Services;

public class DownloadArchiveUseCase : IDownloadArchiveUseCase
{
    private readonly IArchiveService _archiveService;
    private readonly ILogger<DownloadArchiveUseCase> _logger;

    public DownloadArchiveUseCase(
        IArchiveService archiveService,
        ILogger<DownloadArchiveUseCase> logger)
    {
        _archiveService = archiveService;
        _logger = logger;
    }

    public Task<(Stream FileStream, string ContentType, string FileName)> ExecuteAsync(Guid taskId)
    {
        _logger.LogInformation("Executing DownloadArchive use case for task {TaskId}", taskId);

        var task = _archiveService.GetTask(taskId);
        
        if (task == null)
        {
            throw new ApplicationException($"Task with ID {taskId} not found.");
        }

        if (task.Status != ArchiveStatus.Completed)
        {
            throw new ApplicationException(
                $"Archive is not ready. Current status: {task.Status}");
        }

        if (string.IsNullOrEmpty(task.ArchivePath) || !File.Exists(task.ArchivePath))
        {
            throw new ApplicationException("Archive file not found on disk.");
        }

        var fileStream = File.OpenRead(task.ArchivePath);
        
        return Task.FromResult<(Stream FileStream, string ContentType, string FileName)>((fileStream, "application/zip", $"{taskId}.zip"));
    }
}