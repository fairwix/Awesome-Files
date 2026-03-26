using AwesomeFiles.Application.DTOs;
using AwesomeFiles.Application.Interfaces.Services;
using AwesomeFiles.Application.UseCases;
using Microsoft.Extensions.Logging;

namespace AwesomeFiles.Application.Services;
public class GetFilesUseCase : IGetFilesUseCase
{
    private readonly IFileService _fileService;
    private readonly ILogger<GetFilesUseCase> _logger;

    public GetFilesUseCase(IFileService fileService, ILogger<GetFilesUseCase> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<FileDto[]> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Executing GetFiles use case");
        
        var files = await _fileService.GetAllFilesAsync(cancellationToken);
        
        return files
            .Select(f => new FileDto(f))
            .ToArray();
    }
}