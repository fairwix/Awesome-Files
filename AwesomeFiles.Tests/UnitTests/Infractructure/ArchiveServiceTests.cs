using System.IO.Compression;
using AwesomeFiles.Application.Interfaces.Services;
using AwesomeFiles.Domain.Entities;
using AwesomeFiles.Domain.Enums;
using AwesomeFiles.Infrastructure.Options;
using AwesomeFiles.Infrastructure.Services;
using AwesomeFiles.Tests.TestHelpers;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AwesomeFiles.Tests.UnitTests.Infrastructure;

public class ArchiveServiceTests : IDisposable
{
    private readonly string _testArchiveFolder;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly ArchiveService _archiveService;

    public ArchiveServiceTests()
    {
        _testArchiveFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testArchiveFolder);
        
        _fileServiceMock = new Mock<IFileService>();
        var options = Options.Create(new ArchiveStorageOptions { FolderPath = _testArchiveFolder });
        var loggerMock = new Mock<ILogger<ArchiveService>>();
        
        _archiveService = new ArchiveService(_fileServiceMock.Object, options, loggerMock.Object);
    }

    [Fact]
    public void AddTask_ShouldStoreTask()
    {
        var task = TestDataBuilder.CreateArchiveTask(fileNames: new[] { "file1.txt" });
        
        _archiveService.AddTask(task);
        var retrieved = _archiveService.GetTask(task.Id);
        
        retrieved.Should().NotBeNull();
        retrieved!.Id.Should().Be(task.Id);
    }

    [Fact]
    public void GetTask_WhenTaskDoesNotExist_ShouldReturnNull()
    {
        var result = _archiveService.GetTask(Guid.NewGuid());
        
        result.Should().BeNull();
    }

    [Fact]
    public void IsArchiveFileExists_WhenTaskExistsAndFileExists_ShouldReturnTrue()
    {
        var taskId = Guid.NewGuid();
        var archivePath = Path.Combine(_testArchiveFolder, $"{taskId}.zip");
        File.WriteAllText(archivePath, "test");
        
        var task = TestDataBuilder.CreateCompletedTask(taskId, new[] { "file1.txt" }, archivePath);
        _archiveService.AddTask(task);
        
        var result = _archiveService.IsArchiveFileExists(taskId);
        
        result.Should().BeTrue();
    }

    [Fact]
    public void IsArchiveFileExists_WhenTaskExistsButFileMissing_ShouldReturnFalse()
    {
        var taskId = Guid.NewGuid();
        var task = TestDataBuilder.CreateCompletedTask(taskId, new[] { "file1.txt" }, 
            Path.Combine(_testArchiveFolder, "nonexistent.zip"));
        _archiveService.AddTask(task);
        
        var result = _archiveService.IsArchiveFileExists(taskId);
        
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessArchiveTaskAsync_WhenFilesExist_ShouldCreateArchive()
    {
        var testFile = Path.Combine(Path.GetTempPath(), "testfile.txt");
        File.WriteAllText(testFile, "test content");
        
        var task = TestDataBuilder.CreateArchiveTask(fileNames: new[] { "testfile.txt" });
        
        _fileServiceMock
            .Setup(x => x.GetFullPath("testfile.txt"))
            .Returns(testFile);
        
        await _archiveService.ProcessArchiveTaskAsync(task);
        
        task.Status.Should().Be(ArchiveStatus.Completed);
        task.ArchivePath.Should().NotBeNull();
        File.Exists(task.ArchivePath).Should().BeTrue();
        
        using var zip = ZipFile.OpenRead(task.ArchivePath!);
        zip.Entries.Should().HaveCount(1);
        zip.Entries[0].Name.Should().Be("testfile.txt");
        
        File.Delete(testFile);
    }

    [Fact]
    public async Task ProcessArchiveTaskAsync_WhenFileNotFound_ShouldFail()
    {
        var task = TestDataBuilder.CreateArchiveTask(fileNames: new[] { "nonexistent.txt" });
        
        _fileServiceMock
            .Setup(x => x.GetFullPath("nonexistent.txt"))
            .Returns("/nonexistent/path.txt");
        
        await _archiveService.ProcessArchiveTaskAsync(task);
        
        task.Status.Should().Be(ArchiveStatus.Failed);
        task.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ProcessArchiveTaskAsync_WhenCancelled_ShouldFail()
    {
        var task = TestDataBuilder.CreateArchiveTask(fileNames: new[] { "file1.txt" });
        
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        
        await _archiveService.ProcessArchiveTaskAsync(task, cts.Token);
        
        task.Status.Should().Be(ArchiveStatus.Failed);
        task.ErrorMessage.Should().Contain("cancelled");
    }

    [Fact]
    public async Task ProcessArchiveTaskAsync_WhenCached_ShouldUseCache()
    {
        var testFile = Path.Combine(Path.GetTempPath(), "testfile.txt");
        File.WriteAllText(testFile, "test content");
        
        var task1 = TestDataBuilder.CreateArchiveTask(fileNames: new[] { "testfile.txt" });
        var task2 = TestDataBuilder.CreateArchiveTask(fileNames: new[] { "testfile.txt" });
        
        _fileServiceMock
            .Setup(x => x.GetFullPath("testfile.txt"))
            .Returns(testFile);
        
        await _archiveService.ProcessArchiveTaskAsync(task1);
        var firstArchivePath = task1.ArchivePath;
        
        await _archiveService.ProcessArchiveTaskAsync(task2);
        var secondArchivePath = task2.ArchivePath;
        
        firstArchivePath.Should().Be(secondArchivePath);
        
        File.Delete(testFile);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testArchiveFolder))
        {
            Directory.Delete(_testArchiveFolder, true);
        }
    }
}