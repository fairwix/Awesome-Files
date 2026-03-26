using System.Collections.Concurrent;
using System.IO.Compression;
using AwesomeFiles.Application.Interfaces.Services;
using AwesomeFiles.Domain.Entities;
using AwesomeFiles.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AwesomeFiles.Infrastructure.Services;
public class ArchiveService : IArchiveService
{
    private readonly ConcurrentDictionary<Guid, ArchiveTask> _tasks = new();
    private readonly ConcurrentDictionary<string, string> _cache = new();
    private readonly IFileService _fileService;
    private readonly string _archiveFolder;
    private readonly ILogger<ArchiveService> _logger;

    public ArchiveService(
        IFileService fileService,
        IOptions<ArchiveStorageOptions> options,
        ILogger<ArchiveService> logger)
    {
        _fileService = fileService;
        _archiveFolder = options.Value.FolderPath;
        _logger = logger;

        if (!Directory.Exists(_archiveFolder))
        {
            _logger.LogInformation("Creating archive folder: {Folder}", _archiveFolder);
            Directory.CreateDirectory(_archiveFolder);
        }
    }

    public void AddTask(ArchiveTask task)
    {
        _tasks[task.Id] = task;
        _logger.LogDebug("Task {TaskId} added to storage", task.Id);
    }

    public ArchiveTask? GetTask(Guid id)
    {
        _tasks.TryGetValue(id, out var task);
        return task;
    }

    public bool IsArchiveFileExists(Guid taskId)
    {
        var task = GetTask(taskId);
        if (task == null || string.IsNullOrEmpty(task.ArchivePath))
        {
            return false;
        }
        return File.Exists(task.ArchivePath);
    }

    public async Task ProcessArchiveTaskAsync(ArchiveTask task, CancellationToken cancellationToken = default)
    {
        try
        { 
            task.SetInProgress();
            _logger.LogInformation("Processing archive task {TaskId} for {Count} files", 
                task.Id, task.FileNames.Length);
            
            var cacheKey = GetCacheKey(task.FileNames);
            if (_cache.TryGetValue(cacheKey, out var cachedPath) && File.Exists(cachedPath))
            {
                task.SetCompleted(cachedPath); 
                _logger.LogInformation("Using cached archive for task {TaskId}", task.Id);
                return;
            }
            
            var archivePath = Path.Combine(_archiveFolder, $"{task.Id}.zip");
            await CreateArchiveAsync(task, archivePath, cancellationToken);

            task.SetCompleted(archivePath);     
            _cache.TryAdd(cacheKey, archivePath);
        
            _logger.LogInformation("Archive {TaskId} created successfully at {Path}", 
                task.Id, archivePath);
        }
        catch (OperationCanceledException)
        {
            task.SetFailed("Archive creation was cancelled.");
            _logger.LogWarning("Archive task {TaskId} was cancelled", task.Id);
        }
        catch (Exception ex)
        {
            task.SetFailed(ex.Message);
            _logger.LogError(ex, "Archive task {TaskId} failed", task.Id);
        }
    }
    private async Task CreateArchiveAsync(
        ArchiveTask task, 
        string archivePath, 
        CancellationToken cancellationToken)
    {
        using var stream = new FileStream(archivePath, FileMode.Create);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Create);
        
        foreach (var fileName in task.FileNames)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            var fullPath = _fileService.GetFullPath(fileName);
            
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException(
                    $"File '{fileName}' not found at path: {fullPath}");
            }
            
            archive.CreateEntryFromFile(fullPath, fileName);
            _logger.LogDebug("Added {FileName} to archive {TaskId}", fileName, task.Id);
        }
        
        await Task.CompletedTask;
    }

    private static string GetCacheKey(string[] fileNames)
    {
        return string.Join("|", fileNames.OrderBy(x => x));
    }
}