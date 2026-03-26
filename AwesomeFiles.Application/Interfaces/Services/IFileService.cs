namespace AwesomeFiles.Application.Interfaces.Services;

public interface IFileService
{
    Task<string[]> GetAllFilesAsync(CancellationToken cancellationToken = default);
    
    Task<bool> FileExistsAsync(string fileName, CancellationToken cancellationToken = default);
    
    string GetFullPath(string fileName);
}