using AwesomeFiles.Application.DTOs;
using AwesomeFiles.Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using ApplicationException = AwesomeFiles.Application.Exceptions.ApplicationException;
namespace AwesomeFiles.Api.Controllers;


[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IGetFilesUseCase _getFilesUseCase;
    private readonly ICreateArchiveUseCase _createArchiveUseCase;
    private readonly IGetArchiveStatusUseCase _getArchiveStatusUseCase;
    private readonly IDownloadArchiveUseCase _downloadArchiveUseCase;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IGetFilesUseCase getFilesUseCase,
        ICreateArchiveUseCase createArchiveUseCase,
        IGetArchiveStatusUseCase getArchiveStatusUseCase,
        IDownloadArchiveUseCase downloadArchiveUseCase,
        ILogger<FilesController> logger)
    {
        _getFilesUseCase = getFilesUseCase;
        _createArchiveUseCase = createArchiveUseCase;
        _getArchiveStatusUseCase = getArchiveStatusUseCase;
        _downloadArchiveUseCase = downloadArchiveUseCase;
        _logger = logger;
    }

   
    [HttpGet]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    public async Task<ActionResult<string[]>> GetFiles(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GET /api/files called");
        
        var files = await _getFilesUseCase.ExecuteAsync(cancellationToken);
        return Ok(files.Select(f => f.Name).ToArray());
    }
}


[ApiController]
[Route("api/archives")]
public class ArchivesController : ControllerBase
{
    private readonly ICreateArchiveUseCase _createArchiveUseCase;
    private readonly IGetArchiveStatusUseCase _getArchiveStatusUseCase;
    private readonly IDownloadArchiveUseCase _downloadArchiveUseCase;
    private readonly ILogger<ArchivesController> _logger;

    public ArchivesController(
        ICreateArchiveUseCase createArchiveUseCase,
        IGetArchiveStatusUseCase getArchiveStatusUseCase,
        IDownloadArchiveUseCase downloadArchiveUseCase,
        ILogger<ArchivesController> logger)
    {
        _createArchiveUseCase = createArchiveUseCase;
        _getArchiveStatusUseCase = getArchiveStatusUseCase;
        _downloadArchiveUseCase = downloadArchiveUseCase;
        _logger = logger;
    }


    [HttpPost]
    [ProducesResponseType(typeof(CreateArchiveResponse), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateArchiveResponse>> CreateArchive(
        [FromBody] CreateArchiveRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("POST /api/archives called with {Count} files", 
            request.FileNames?.Length ?? 0);

        var response = await _createArchiveUseCase.ExecuteAsync(request, cancellationToken);
        
        return AcceptedAtAction(nameof(GetStatus), new { id = response.Id }, response);
    }
    
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ArchiveStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ArchiveStatusResponse>> GetStatus(Guid id)
    {
        _logger.LogInformation("GET /api/archives/{Id} called", id);

        try
        {
            var response = await _getArchiveStatusUseCase.ExecuteAsync(id);
            return Ok(response);
        }
        catch (ApplicationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Task {Id} not found", id);
            return NotFound(new { error = ex.Message });
        }
    }
    
    [HttpGet("{id}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DownloadArchive(Guid id)
    {
        _logger.LogInformation("GET /api/archives/{Id}/download called", id);

        try
        {
            var (fileStream, contentType, fileName) = await _downloadArchiveUseCase.ExecuteAsync(id);
            return File(fileStream, contentType, fileName);
        }
        catch (ApplicationException ex) when (ex.Message.Contains("not found"))
        {
            _logger.LogWarning("Task {Id} not found", id);
            return NotFound(new { error = ex.Message });
        }
    }
}