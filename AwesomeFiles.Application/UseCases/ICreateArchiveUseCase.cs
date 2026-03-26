using AwesomeFiles.Application.DTOs;

namespace AwesomeFiles.Application.UseCases;

public interface ICreateArchiveUseCase
{
    Task<CreateArchiveResponse> ExecuteAsync(
        CreateArchiveRequest request, 
        CancellationToken cancellationToken = default);
}