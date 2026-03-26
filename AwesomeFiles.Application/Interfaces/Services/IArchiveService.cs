using AwesomeFiles.Domain.Entities;

namespace AwesomeFiles.Application.Interfaces.Services;

public interface IArchiveService
{
    void AddTask(ArchiveTask task);
    ArchiveTask? GetTask(Guid id);

    bool IsArchiveFileExists(Guid taskId);
    
    Task ProcessArchiveTaskAsync(ArchiveTask task, CancellationToken cancellationToken = default);
}