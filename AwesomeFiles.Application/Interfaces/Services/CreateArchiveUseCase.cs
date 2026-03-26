using AwesomeFiles.Application.DTOs;
using AwesomeFiles.Application.Exceptions;
using AwesomeFiles.Application.Interfaces;
using AwesomeFiles.Application.Interfaces.Services;
using AwesomeFiles.Application.UseCases;
using AwesomeFiles.Domain.Entities;
using AwesomeFiles.Domain.Enums;
using Microsoft.Extensions.Logging;
using ApplicationException = AwesomeFiles.Application.Exceptions.ApplicationException;

namespace AwesomeFiles.Application.Services;

public class CreateArchiveUseCase : ICreateArchiveUseCase
{
    private readonly IFileService _fileService;
    private readonly IArchiveService _archiveService;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<CreateArchiveUseCase> _logger;
    private const int MaxFiles = 50;

    public CreateArchiveUseCase(
        IFileService fileService,
        IArchiveService archiveService,
        IBackgroundTaskQueue taskQueue,
        ILogger<CreateArchiveUseCase> logger)
    {
        _fileService = fileService;
        _archiveService = archiveService;
        _taskQueue = taskQueue;
        _logger = logger;
    }

    public async Task<CreateArchiveResponse> ExecuteAsync(
        CreateArchiveRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ApplicationException("Request cannot be null.");
        }
        _logger.LogInformation("Executing CreateArchive use case for files: {FileNames}", 
            string.Join(", ", request.FileNames));
        
        if (request.FileNames == null || request.FileNames.Length == 0)
        {
            throw new ApplicationException("At least one file name must be provided.");
        }
        
        if (request.FileNames.Length > MaxFiles)
        {
            _logger.LogWarning("Too many files requested: {Count} (max: {MaxFiles})", 
                request.FileNames.Length, MaxFiles);
            throw new ApplicationException($"Maximum {MaxFiles} files per request allowed.");
        }
        
        var invalidFiles = new List<string>();
        foreach (var fileName in request.FileNames)
        {
            if (!await _fileService.FileExistsAsync(fileName, cancellationToken))
            {
                invalidFiles.Add(fileName);
            }
        }

        if (invalidFiles.Any())
        {
            _logger.LogWarning("Invalid files detected: {InvalidFiles}", invalidFiles);
            throw new ApplicationException(
                $"The following files do not exist: {string.Join(", ", invalidFiles)}");
        }
        
        var taskId = Guid.NewGuid();
        var task = new ArchiveTask(taskId, request.FileNames);
        
        _archiveService.AddTask(task);
        
        _taskQueue.QueueBackgroundWorkItem(async token =>
        {
            _logger.LogInformation("Starting background processing for task {TaskId}", taskId);
            await _archiveService.ProcessArchiveTaskAsync(task, token);
        });

        _logger.LogInformation("Archive task {TaskId} created successfully", taskId);
        
        return new CreateArchiveResponse(taskId);
    }
}