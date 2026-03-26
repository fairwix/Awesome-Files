using AwesomeFiles.Domain.Enums;

namespace AwesomeFiles.Domain.Entities;

public class ArchiveTask
{
    private readonly object _statusLock = new();

    public Guid Id { get; set; }
    public string[] FileNames { get; set; } = Array.Empty<string>();
    public ArchiveStatus Status { get; private set; }
    public string? ArchivePath { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; set; }
    
    public ArchiveTask(Guid id, string[] fileNames)
    {
        if (id == Guid.Empty)
            throw new ArgumentException("Id cannot be empty", nameof(id));
        if (fileNames == null || fileNames.Length == 0)
            throw new ArgumentException("At least one file name must be provided", nameof(fileNames));

        Id = id;
        FileNames = fileNames;
        Status = ArchiveStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetInProgress()
    {
        lock (_statusLock)
        {
            Status = ArchiveStatus.InProgress;
        }
    }

    public void SetCompleted(string archivePath)
    {
        if (string.IsNullOrWhiteSpace(archivePath))
            throw new ArgumentException("Archive path cannot be empty.", nameof(archivePath));

        lock (_statusLock)
        {
            Status = ArchiveStatus.Completed;
            ArchivePath = archivePath;
        }
    }

    public void SetFailed(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty.", nameof(errorMessage));

        lock (_statusLock)
        {
            Status = ArchiveStatus.Failed;
            ErrorMessage = errorMessage;
        }
    }
}