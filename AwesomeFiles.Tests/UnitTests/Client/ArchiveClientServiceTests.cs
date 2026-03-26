using AwesomeFiles.Client.Application;
using AwesomeFiles.Client.Infrastructure;
using AwesomeFiles.Client.Models;
using FluentAssertions;
using Moq;

namespace AwesomeFiles.Tests.UnitTests.Client;

public class ArchiveClientServiceTests
{
    private readonly Mock<IApiClient> _apiClientMock;
    private readonly ArchiveClientService _service;

    public ArchiveClientServiceTests()
    {
        _apiClientMock = new Mock<IApiClient>();
        _service = new ArchiveClientService(_apiClientMock.Object);
    }

    [Fact]
    public async Task CreateAndWaitAndDownloadAsync_ShouldCreateWaitAndDownload()
    {
        var taskId = Guid.NewGuid();
        var fileNames = new[] { "file1.txt", "file2.txt" };
        var destinationFolder = "/tmp";
        
        _apiClientMock
            .Setup(x => x.CreateArchiveAsync(fileNames, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateArchiveResponse(taskId));
        
        _apiClientMock
            .Setup(x => x.GetStatusAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusResponse(taskId, "Completed", null));
        
        _apiClientMock
            .Setup(x => x.DownloadArchiveAsync(taskId, destinationFolder, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        
        await _service.CreateAndWaitAndDownloadAsync(fileNames, destinationFolder);
        
        _apiClientMock.Verify(x => x.CreateArchiveAsync(fileNames, It.IsAny<CancellationToken>()), Times.Once);
        _apiClientMock.Verify(x => x.GetStatusAsync(taskId, It.IsAny<CancellationToken>()), Times.Once);
        _apiClientMock.Verify(x => x.DownloadArchiveAsync(taskId, destinationFolder, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WaitForCompletionAsync_ShouldPollUntilCompleted()
    {
        var taskId = Guid.NewGuid();
        var callCount = 0;
        
        _apiClientMock
            .Setup(x => x.GetStatusAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                callCount++;
                return callCount switch
                {
                    1 => new StatusResponse(taskId, "Pending", null),
                    2 => new StatusResponse(taskId, "InProgress", null),
                    _ => new StatusResponse(taskId, "Completed", null)
                };
            });
        
        await _service.WaitForCompletionAsync(taskId);
        
        callCount.Should().Be(3);
    }

    [Fact]
    public async Task WaitForCompletionAsync_WhenFailed_ShouldThrow()
    {
        var taskId = Guid.NewGuid();
        _apiClientMock
            .Setup(x => x.GetStatusAsync(taskId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusResponse(taskId, "Failed", "Something went wrong"));
        
        var exception = await Assert.ThrowsAsync<Exception>(
            () => _service.WaitForCompletionAsync(taskId));
        
        exception.Message.Should().Contain("failed");
    }
}