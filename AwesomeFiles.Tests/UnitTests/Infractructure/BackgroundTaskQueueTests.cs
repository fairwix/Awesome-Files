using AwesomeFiles.Infrastructure.Services;
using FluentAssertions;

namespace AwesomeFiles.Tests.UnitTests.Infrastructure;

public class BackgroundTaskQueueTests
{
    [Fact]
    public async Task QueueBackgroundWorkItem_ShouldProcessItem()
    {
        var queue = new BackgroundTaskQueue();
        var executed = false;
        
        queue.QueueBackgroundWorkItem(async token =>
        {
            executed = true;
            await Task.CompletedTask;
        });
        
        var workItem = await queue.DequeueAsync(CancellationToken.None);
        await workItem(CancellationToken.None);
        
        executed.Should().BeTrue();
    }
    
    [Fact]
    public async Task QueueBackgroundWorkItem_ShouldProcessMultipleItemsInOrder()
    {
        var queue = new BackgroundTaskQueue();
        var executionOrder = new List<int>();
        
        queue.QueueBackgroundWorkItem(async token =>
        {
            executionOrder.Add(1);
            await Task.CompletedTask;
        });
        
        queue.QueueBackgroundWorkItem(async token =>
        {
            executionOrder.Add(2);
            await Task.CompletedTask;
        });
        
        var workItem1 = await queue.DequeueAsync(CancellationToken.None);
        await workItem1(CancellationToken.None);
        
        var workItem2 = await queue.DequeueAsync(CancellationToken.None);
        await workItem2(CancellationToken.None);
        
        executionOrder.Should().BeEquivalentTo(new[] { 1, 2 });
    }
    
    [Fact]
    public async Task DequeueAsync_ShouldWaitForItem()
    {
        var queue = new BackgroundTaskQueue();
        var dequeueTask = queue.DequeueAsync(CancellationToken.None);
        var executed = false;
        
        queue.QueueBackgroundWorkItem(async token =>
        {
            executed = true;
            await Task.CompletedTask;
        });
        
        var workItem = await dequeueTask;
        await workItem(CancellationToken.None);
        
        executed.Should().BeTrue();
    }
    
    [Fact]
    public void QueueBackgroundWorkItem_WithNullWorkItem_ShouldThrow()
    {
        var queue = new BackgroundTaskQueue();
 
        Assert.Throws<ArgumentNullException>(() => 
            queue.QueueBackgroundWorkItem(null!));
    }
}