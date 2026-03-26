using AwesomeFiles.Application.DTOs;

namespace AwesomeFiles.Application.UseCases;

public interface IGetArchiveStatusUseCase
{
    Task<ArchiveStatusResponse> ExecuteAsync(Guid taskId);
}