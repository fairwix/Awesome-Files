using AwesomeFiles.Application.DTOs;
using AwesomeFiles.Application.Exceptions;
using AwesomeFiles.Application.Interfaces;
using AwesomeFiles.Application.Interfaces.Services;
using AwesomeFiles.Application.Services;
using AwesomeFiles.Domain.Entities;
using AwesomeFiles.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using ApplicationException = AwesomeFiles.Application.Exceptions.ApplicationException;

namespace AwesomeFiles.Tests.UnitTests.Application;

public class CreateArchiveUseCaseTests
{
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IArchiveService> _archiveServiceMock;
    private readonly Mock<IBackgroundTaskQueue> _taskQueueMock;
    private readonly Mock<ILogger<CreateArchiveUseCase>> _loggerMock;
    private readonly CreateArchiveUseCase _useCase;

    public CreateArchiveUseCaseTests()
    {
        _fileServiceMock = new Mock<IFileService>();
        _archiveServiceMock = new Mock<IArchiveService>();
        _taskQueueMock = new Mock<IBackgroundTaskQueue>();
        _loggerMock = new Mock<ILogger<CreateArchiveUseCase>>();
        
        _useCase = new CreateArchiveUseCase(
            _fileServiceMock.Object,
            _archiveServiceMock.Object,
            _taskQueueMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidFiles_ShouldCreateTaskAndReturnId()
    {
        var request = TestDataBuilder.CreateArchiveRequest(new[] { "file1.txt", "file2.txt" });
        _fileServiceMock.SetupFileServiceWithAllFilesExist(request.FileNames);
        
        var result = await _useCase.ExecuteAsync(request);
        
        result.Id.Should().NotBeEmpty();
        
        _archiveServiceMock.Verify(
            x => x.AddTask(It.Is<ArchiveTask>(t => 
                t.FileNames.Length == 2 && 
                t.FileNames.Contains("file1.txt") &&
                t.FileNames.Contains("file2.txt"))), 
            Times.Once);
        
        _taskQueueMock.Verify(
            x => x.QueueBackgroundWorkItem(It.IsAny<Func<CancellationToken, Task>>()), 
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyFileList_ShouldThrowApplicationException()
    {
        var request = TestDataBuilder.CreateEmptyRequest();
        
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _useCase.ExecuteAsync(request));
        
        exception.Message.Should().Contain("At least one file name");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentFiles_ShouldThrowApplicationException()
    {
        var request = TestDataBuilder.CreateArchiveRequest(new[] { "file1.txt", "nonexistent.txt" });
        _fileServiceMock.SetupFileServiceWithFileExistence(new Dictionary<string, bool>
        {
            ["file1.txt"] = true,
            ["nonexistent.txt"] = false
        });
        
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _useCase.ExecuteAsync(request));
        
        exception.Message.Should().Contain("nonexistent.txt");
        exception.Message.Should().NotContain("file1.txt");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullRequest_ShouldThrowApplicationException()
    {
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _useCase.ExecuteAsync(null!));
        
        exception.Message.Should().Contain("Request cannot be null");
    }

    [Fact]
    public async Task ExecuteAsync_WithNullFileNames_ShouldThrowArgumentNullException()
    {
        var request = TestDataBuilder.CreateNullRequest();
        
        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => _useCase.ExecuteAsync(request));
    
        exception.ParamName.Should().Be("value");
    }
}