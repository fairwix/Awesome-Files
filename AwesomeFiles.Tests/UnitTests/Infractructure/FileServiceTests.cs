using AwesomeFiles.Infrastructure.Options;
using AwesomeFiles.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace AwesomeFiles.Tests.UnitTests.Infrastructure;

public class FileServiceTests : IDisposable
{
    private readonly string _testFolder;
    private readonly FileService _fileService;

    public FileServiceTests()
    {
        _testFolder = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testFolder);
        
        var options = Options.Create(new FileStorageOptions { FolderPath = _testFolder });
        var loggerMock = new Mock<ILogger<FileService>>();
        
        _fileService = new FileService(options, loggerMock.Object);
    }

    [Fact]
    public async Task GetAllFilesAsync_WhenFilesExist_ShouldReturnAllFiles()
    {
        await File.WriteAllTextAsync(Path.Combine(_testFolder, "file1.txt"), "content");
        await File.WriteAllTextAsync(Path.Combine(_testFolder, "file2.txt"), "content");
        await File.WriteAllTextAsync(Path.Combine(_testFolder, "file3.txt"), "content");
        
        var result = await _fileService.GetAllFilesAsync();
        
        result.Should().HaveCount(3);
        result.Should().Contain("file1.txt");
        result.Should().Contain("file2.txt");
        result.Should().Contain("file3.txt");
    }

    [Fact]
    public async Task GetAllFilesAsync_WhenNoFiles_ShouldReturnEmptyArray()
    {
        var result = await _fileService.GetAllFilesAsync();
        
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task FileExistsAsync_WhenFileExists_ShouldReturnTrue()
    {
        await File.WriteAllTextAsync(Path.Combine(_testFolder, "file1.txt"), "content");
        
        var result = await _fileService.FileExistsAsync("file1.txt");
        
        result.Should().BeTrue();
    }

    [Fact]
    public async Task FileExistsAsync_WhenFileDoesNotExist_ShouldReturnFalse()
    {
        var result = await _fileService.FileExistsAsync("nonexistent.txt");
        
        result.Should().BeFalse();
    }

    [Fact]
    public void GetFullPath_ShouldReturnCorrectPath()
    {
        var result = _fileService.GetFullPath("test.txt");
        
        result.Should().Be(Path.Combine(_testFolder, "test.txt"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_testFolder))
        {
            Directory.Delete(_testFolder, true);
        }
    }
}