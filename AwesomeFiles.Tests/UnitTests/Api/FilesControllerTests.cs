using AwesomeFiles.Api.Controllers;
using AwesomeFiles.Application.DTOs;
using AwesomeFiles.Application.Exceptions;
using AwesomeFiles.Application.UseCases;
using AwesomeFiles.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ApplicationException = AwesomeFiles.Application.Exceptions.ApplicationException;

namespace AwesomeFiles.Tests.UnitTests.Api;

public class ArchivesControllerTests
{
    private readonly Mock<ICreateArchiveUseCase> _createArchiveUseCaseMock;
    private readonly Mock<IGetArchiveStatusUseCase> _getStatusUseCaseMock;
    private readonly Mock<IDownloadArchiveUseCase> _downloadUseCaseMock;
    private readonly ArchivesController _controller;

    public ArchivesControllerTests()
    {
        _createArchiveUseCaseMock = new Mock<ICreateArchiveUseCase>();
        _getStatusUseCaseMock = new Mock<IGetArchiveStatusUseCase>();
        _downloadUseCaseMock = new Mock<IDownloadArchiveUseCase>();
        var loggerMock = new Mock<ILogger<ArchivesController>>();
        
        _controller = new ArchivesController(
            _createArchiveUseCaseMock.Object,
            _getStatusUseCaseMock.Object,
            _downloadUseCaseMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public async Task CreateArchive_WithValidRequest_ShouldReturnAcceptedAtAction()
    {
        var request = TestDataBuilder.CreateArchiveRequest(new[] { "file1.txt", "file2.txt" });
        var response = new CreateArchiveResponse(Guid.NewGuid());
        
        _createArchiveUseCaseMock
            .Setup(x => x.ExecuteAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        
        var result = await _controller.CreateArchive(request, CancellationToken.None);
        
        var acceptedResult = result.Result.Should().BeOfType<AcceptedAtActionResult>().Subject;
        acceptedResult.ActionName.Should().Be(nameof(ArchivesController.GetStatus));
        acceptedResult.RouteValues!["id"].Should().Be(response.Id);
        var returnedResponse = acceptedResult.Value.Should().BeOfType<CreateArchiveResponse>().Subject;
        returnedResponse.Id.Should().Be(response.Id);
    }

    [Fact]
    public async Task GetStatus_WhenTaskExists_ShouldReturnOk()
    {
        var taskId = Guid.NewGuid();
        var statusResponse = new ArchiveStatusResponse(taskId, "Completed");
        
        _getStatusUseCaseMock
            .Setup(x => x.ExecuteAsync(taskId))
            .ReturnsAsync(statusResponse);
        
        var result = await _controller.GetStatus(taskId);
        
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var returnedStatus = okResult.Value.Should().BeOfType<ArchiveStatusResponse>().Subject;
        returnedStatus.Id.Should().Be(taskId);
        returnedStatus.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task GetStatus_WhenTaskNotFound_ShouldReturnNotFound()
    {
        var taskId = Guid.NewGuid();
        _getStatusUseCaseMock
            .Setup(x => x.ExecuteAsync(taskId))
            .ThrowsAsync(new ApplicationException("Task not found"));
        
        var result = await _controller.GetStatus(taskId);
        
        var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFoundResult.Value.Should().BeEquivalentTo(new { error = "Task not found" });
    }

    [Fact]
    public async Task DownloadArchive_WhenReady_ShouldReturnFile()
    {
        var taskId = Guid.NewGuid();
        var stream = new MemoryStream();
        _downloadUseCaseMock
            .Setup(x => x.ExecuteAsync(taskId))
            .ReturnsAsync((stream, "application/zip", $"{taskId}.zip"));
        
        var result = await _controller.DownloadArchive(taskId);
        
        var fileResult = result.Should().BeOfType<FileStreamResult>().Subject;
        fileResult.ContentType.Should().Be("application/zip");
        fileResult.FileDownloadName.Should().Be($"{taskId}.zip");
    }
}