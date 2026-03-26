using AwesomeFiles.Application.DTOs;

namespace AwesomeFiles.Application.UseCases;

public interface IGetFilesUseCase
{
    Task<FileDto[]> ExecuteAsync(CancellationToken cancellationToken = default);
}