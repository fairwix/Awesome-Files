using AwesomeFiles.Domain.Entities;
using AwesomeFiles.Domain.Enums;
using AwesomeFiles.Application.DTOs;
using AwesomeFiles.Application.Interfaces.Services;
using Moq;

namespace AwesomeFiles.Tests.TestHelpers;
public static class TestDataBuilder
{
    public static ArchiveTask CreateArchiveTask(
        Guid? id = null,
        string[]? fileNames = null)
    {
        return new ArchiveTask(
            id ?? Guid.NewGuid(),
            fileNames ?? new[] { "default.txt" });
    }
    
    public static ArchiveTask CreateCompletedTask(
        Guid? id = null,
        string[]? fileNames = null,
        string? archivePath = null)
    {
        var task = CreateArchiveTask(id, fileNames);
        task.SetCompleted(archivePath ?? $"/tmp/{task.Id}.zip");
        return task;
    }
    
    public static ArchiveTask CreateFailedTask(
        Guid? id = null,
        string[]? fileNames = null,
        string? errorMessage = null)
    {
        var task = CreateArchiveTask(id, fileNames);
        task.SetFailed(errorMessage ?? "Test error");
        return task;
    }
    
    public static ArchiveTask CreateInProgressTask(
        Guid? id = null,
        string[]? fileNames = null)
    {
        var task = CreateArchiveTask(id, fileNames);
        task.SetInProgress();
        return task;
    }
    
    public static Mock<IFileService> SetupFileServiceWithAllFilesExist(
        this Mock<IFileService> mock,
        string[] fileNames)
    {
        foreach (var fileName in fileNames)
        {
            mock.Setup(x => x.FileExistsAsync(fileName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }
        return mock;
    }
    
    public static Mock<IFileService> SetupFileServiceWithFileExistence(
        this Mock<IFileService> mock,
        Dictionary<string, bool> fileExistsMap)
    {
        foreach (var (fileName, exists) in fileExistsMap)
        {
            mock.Setup(x => x.FileExistsAsync(fileName, It.IsAny<CancellationToken>()))
                .ReturnsAsync(exists);
        }
        return mock;
    }
    
    public static CreateArchiveRequest CreateArchiveRequest(string[]? fileNames = null)
    {
        return new CreateArchiveRequest(fileNames ?? new[] { "file1.txt", "file2.txt" });
    }
    
    public static CreateArchiveRequest CreateSingleFileRequest(string fileName = "file1.txt")
    {
        return new CreateArchiveRequest(new[] { fileName });
    }
    
    public static CreateArchiveRequest CreateEmptyRequest()
    {
        return new CreateArchiveRequest(Array.Empty<string>());
    }
    
    public static CreateArchiveRequest CreateNullRequest()
    {
        return new CreateArchiveRequest(null!);
    }
}