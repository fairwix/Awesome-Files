using AwesomeFiles.Application.Interfaces;
using AwesomeFiles.Infrastructure.BackgroundServices;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AwesomeFiles.Tests.UnitTests.Infrastructure;

public class ArchiveWorkerTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldProcessWorkItemsFromQueue()
    {
        var taskQueueMock = new Mock<IBackgroundTaskQueue>();
        var loggerMock = new Mock<ILogger<ArchiveWorker>>();
        
        var semaphore = new SemaphoreSlim(0, 1);
        var workItemExecuted = false;
        
        taskQueueMock
            .Setup(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((CancellationToken token) =>
            {
                workItemExecuted = true;
                semaphore.Release();
                return Task.CompletedTask;
            });

        var worker = new ArchiveWorker(taskQueueMock.Object, loggerMock.Object);
        
        using var cts = new CancellationTokenSource();
        
        var task = worker.StartAsync(cts.Token);
        
        var completed = await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
        
        cts.Cancel();
        
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
            
        }
        
        completed.Should().BeTrue();
        workItemExecuted.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteAsync_WhenWorkItemThrows_ShouldContinue()
    {
        var queueMock = new Mock<IBackgroundTaskQueue>();
        var loggerMock = new Mock<ILogger<ArchiveWorker>>();
        
        var semaphore = new SemaphoreSlim(0, 2);
        var callCount = 0;
        
        queueMock.Setup(x => x.DequeueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((CancellationToken token) =>
            {
                callCount++;
                semaphore.Release();
                
                if (callCount == 1)
                    throw new Exception("Test error");
                    
                return Task.CompletedTask;
            });
    
        var worker = new ArchiveWorker(queueMock.Object, loggerMock.Object);
        using var cts = new CancellationTokenSource();
        var task = worker.StartAsync(cts.Token);
        
        var firstCompleted = await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
        var secondCompleted = await semaphore.WaitAsync(TimeSpan.FromSeconds(5));
        
        cts.Cancel();
        
        try
        {
            await task;
        }
        catch (OperationCanceledException)
        {
        }
    
        callCount.Should().BeGreaterThan(1);
        firstCompleted.Should().BeTrue();
        secondCompleted.Should().BeTrue();
    }
}