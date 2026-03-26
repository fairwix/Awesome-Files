using AwesomeFiles.Application.DTOs;
using AwesomeFiles.Application.Interfaces.Services;
using AwesomeFiles.Application.Services;
using AwesomeFiles.Application.UseCases;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AwesomeFiles.Tests.UnitTests.Application;

public class GetFilesUseCaseTests
{
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<ILogger<GetFilesUseCase>> _loggerMock;
    private readonly GetFilesUseCase _useCase;

    public GetFilesUseCaseTests()
    {
        _fileServiceMock = new Mock<IFileService>();
        _loggerMock = new Mock<ILogger<GetFilesUseCase>>();
        _useCase = new GetFilesUseCase(_fileServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenFilesExist_ShouldReturnFileDtos()
    {
        var expectedFiles = new[] { "file1.txt", "file2.txt", "file3.txt" };
        _fileServiceMock
            .Setup(x => x.GetAllFilesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedFiles);
        
        var result = await _useCase.ExecuteAsync();
        
        result.Should().HaveCount(3);
        result.Should().AllBeOfType<FileDto>();
        result.Select(f => f.Name).Should().BeEquivalentTo(expectedFiles);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoFiles_ShouldReturnEmptyArray()
    {
        _fileServiceMock
            .Setup(x => x.GetAllFilesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<string>());
        
        var result = await _useCase.ExecuteAsync();
        
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteAsync_WhenFileServiceThrows_ShouldPropagateException()
    {
        _fileServiceMock
            .Setup(x => x.GetAllFilesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new IOException("Disk error"));
        
        await Assert.ThrowsAsync<IOException>(() => _useCase.ExecuteAsync());
    }
}