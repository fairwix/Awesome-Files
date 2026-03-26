namespace AwesomeFiles.Application.UseCases;

public interface IDownloadArchiveUseCase
{
    Task<(Stream FileStream, string ContentType, string FileName)> ExecuteAsync(Guid taskId);
}