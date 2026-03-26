using AwesomeFiles.Application.DTOs;
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

public class GetArchiveStatusUseCaseTests
{
    private readonly Mock<IArchiveService> _archiveServiceMock;
    private readonly Mock<ILogger<GetArchiveStatusUseCase>> _loggerMock;
    private readonly GetArchiveStatusUseCase _useCase;

    public GetArchiveStatusUseCaseTests()
    {
        _archiveServiceMock = new Mock<IArchiveService>();
        _loggerMock = new Mock<ILogger<GetArchiveStatusUseCase>>();
        _useCase = new GetArchiveStatusUseCase(_archiveServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskExists_ShouldReturnStatus()
    {
        var taskId = Guid.NewGuid();
        var task = TestDataBuilder.CreateCompletedTask(taskId, new[] { "file1.txt" });
        
        _archiveServiceMock
            .Setup(x => x.GetTask(taskId))
            .Returns(task);
        
        var result = await _useCase.ExecuteAsync(taskId);
        
        result.Id.Should().Be(taskId);
        result.Status.Should().Be("Completed");
        result.Error.Should().BeNull();
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskNotFound_ShouldThrowApplicationException()
    {
        var taskId = Guid.NewGuid();
        _archiveServiceMock
            .Setup(x => x.GetTask(taskId))
            .Returns((ArchiveTask?)null);
        
        var exception = await Assert.ThrowsAsync<ApplicationException>(
            () => _useCase.ExecuteAsync(taskId));
        
        exception.Message.Should().Contain(taskId.ToString());
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskFailed_ShouldReturnError()
    {
        var taskId = Guid.NewGuid();
        var task = TestDataBuilder.CreateFailedTask(taskId, new[] { "file1.txt" }, "Something went wrong");
        
        _archiveServiceMock
            .Setup(x => x.GetTask(taskId))
            .Returns(task);
        
        var result = await _useCase.ExecuteAsync(taskId);
        
        result.Status.Should().Be("Failed");
        result.Error.Should().Be("Something went wrong");
    }

    [Fact]
    public async Task ExecuteAsync_WhenTaskInProgress_ShouldReturnInProgress()
    {
        var taskId = Guid.NewGuid();
        var task = TestDataBuilder.CreateInProgressTask(taskId, new[] { "file1.txt" });
        
        _archiveServiceMock
            .Setup(x => x.GetTask(taskId))
            .Returns(task);
        
        var result = await _useCase.ExecuteAsync(taskId);
        
        result.Status.Should().Be("InProgress");
        result.Error.Should().BeNull();
    }
}