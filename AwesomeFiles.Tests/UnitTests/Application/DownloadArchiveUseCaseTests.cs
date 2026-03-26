using AwesomeFiles.Application.Exceptions;
using AwesomeFiles.Application.Interfaces.Services;
using AwesomeFiles.Application.Services;
using AwesomeFiles.Domain.Entities;
using AwesomeFiles.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ApplicationException = AwesomeFiles.Application.Exceptions.ApplicationException;

namespace AwesomeFiles.Tests.UnitTests.Application;

public class DownloadArchiveUseCaseTests
{
    private readonly Mock<IArchiveService> _archiveServiceMock;
    private readonly Mock<ILogger<DownloadArchiveUseCase>> _loggerMock;
    private readonly DownloadArchiveUseCase _useCase;

    public DownloadArchiveUseCaseTests()
    {
        _archiveServiceMock = new Mock<IArchiveService>();
        _loggerMock = new Mock<ILogger<DownloadArchiveUseCase>>();
        _useCase = new DownloadArchiveUseCase(_archiveServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskCompletedAndArchiveExists_ShouldReturnStream()
    {
        var taskId = Guid.NewGuid();
        var archivePath = Path.GetTempFileName();
        var task = TestDataBuilder.CreateCompletedTask(taskId, new[] { "file1.txt" }, archivePath);
        
        _archiveServiceMock.Setup(x => x.GetTask(taskId)).Returns(task);
        _archiveServiceMock.Setup(x => x.IsArchiveFileExists(taskId)).Returns(true);
        
        var (fileStream, contentType, fileName) = await _useCase.ExecuteAsync(taskId);
        
        fileStream.Should().NotBeNull();
        contentType.Should().Be("application/zip");
        fileName.Should().Be($"{taskId}.zip");
        
        fileStream.Dispose();
        File.Delete(archivePath);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskNotFound_ShouldThrowApplicationException()
    {
        var taskId = Guid.NewGuid();
        _archiveServiceMock.Setup(x => x.GetTask(taskId)).Returns((ArchiveTask?)null);
        
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _useCase.ExecuteAsync(taskId));
        
        exception.Message.Should().Contain(taskId.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskNotCompleted_ShouldThrowApplicationException()
    {
        var taskId = Guid.NewGuid();
        var task = TestDataBuilder.CreateInProgressTask(taskId, new[] { "file1.txt" });
        
        _archiveServiceMock.Setup(x => x.GetTask(taskId)).Returns(task);
        
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _useCase.ExecuteAsync(taskId));
        
        exception.Message.Should().Contain("not ready");
        exception.Message.Should().Contain("InProgress");
    }

    [Fact]
    public async Task ExecuteAsync_WhenArchiveFileMissing_ShouldThrowApplicationException()
    {
        var taskId = Guid.NewGuid();
        var task = TestDataBuilder.CreateCompletedTask(taskId, new[] { "file1.txt" }, "/nonexistent/path.zip");
        
        _archiveServiceMock.Setup(x => x.GetTask(taskId)).Returns(task);
        _archiveServiceMock.Setup(x => x.IsArchiveFileExists(taskId)).Returns(false);
        
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _useCase.ExecuteAsync(taskId));
        
        exception.Message.Should().Contain("not found");
    }
}