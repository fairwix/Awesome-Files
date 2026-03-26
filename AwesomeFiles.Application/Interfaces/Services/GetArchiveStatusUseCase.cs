using AwesomeFiles.Application.DTOs;
using AwesomeFiles.Application.Exceptions;
using AwesomeFiles.Application.Interfaces.Services;
using AwesomeFiles.Application.UseCases;
using Microsoft.Extensions.Logging;
using ApplicationException = AwesomeFiles.Application.Exceptions.ApplicationException;

namespace AwesomeFiles.Application.Services;


public class GetArchiveStatusUseCase : IGetArchiveStatusUseCase
{
    private readonly IArchiveService _archiveService;
    private readonly ILogger<GetArchiveStatusUseCase> _logger;

    public GetArchiveStatusUseCase(
        IArchiveService archiveService,
        ILogger<GetArchiveStatusUseCase> logger)
    {
        _archiveService = archiveService;
        _logger = logger;
    }

    public Task<ArchiveStatusResponse> ExecuteAsync(Guid taskId)
    {
        _logger.LogInformation("Executing GetArchiveStatus use case for task {TaskId}", taskId);

        var task = _archiveService.GetTask(taskId);
        
        if (task == null)
        {
            throw new ApplicationException($"Task with ID {taskId} not found.");
        }

        return Task.FromResult(new ArchiveStatusResponse(
            task.Id, 
            task.Status.ToString(), 
            task.ErrorMessage));
    }
}