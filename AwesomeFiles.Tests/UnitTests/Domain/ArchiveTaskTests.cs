using AwesomeFiles.Domain.Entities;
using AwesomeFiles.Domain.Enums;
using FluentAssertions;

namespace AwesomeFiles.Tests.UnitTests.Domain;

public class ArchiveTaskTests
{
    [Fact]
    public void Constructor_WithValidData_ShouldCreateTask()
    {
        var id = Guid.NewGuid();
        var fileNames = new[] { "file1.txt", "file2.txt" };
        
        var task = new ArchiveTask(id, fileNames);
        
        task.Id.Should().Be(id);
        task.FileNames.Should().BeEquivalentTo(fileNames);
        task.Status.Should().Be(ArchiveStatus.Pending);
        task.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        task.ArchivePath.Should().BeNull();
        task.ErrorMessage.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithEmptyId_ShouldThrow()
    {
        var id = Guid.Empty;
        var fileNames = new[] { "file1.txt" };
        
        Action act = () => new ArchiveTask(id, fileNames);
        
        act.Should().Throw<ArgumentException>().WithMessage("*Id cannot be empty*");
    }

    [Fact]
    public void Constructor_WithEmptyFileNames_ShouldThrow()
    {
        var id = Guid.NewGuid();
        var fileNames = Array.Empty<string>();
        
        Action act = () => new ArchiveTask(id, fileNames);
        
        act.Should().Throw<ArgumentException>().WithMessage("*At least one file name*");
    }

    [Fact]
    public void Constructor_WithNullFileNames_ShouldThrow()
    {
        var id = Guid.NewGuid();
        
        Action act = () => new ArchiveTask(id, null!);
        
        act.Should().Throw<ArgumentException>().WithMessage("*At least one file name*");
    }

    [Fact]
    public void SetInProgress_ShouldChangeStatus()
    {
        var task = new ArchiveTask(Guid.NewGuid(), new[] { "file1.txt" });
        
        task.SetInProgress();
        
        task.Status.Should().Be(ArchiveStatus.InProgress);
    }

    [Fact]
    public void SetCompleted_ShouldChangeStatusAndSetPath()
    {
        // Arrange
        var task = new ArchiveTask(Guid.NewGuid(), new[] { "file1.txt" });
        var archivePath = "/path/to/archive.zip";

        // Act
        task.SetCompleted(archivePath);

        // Assert
        task.Status.Should().Be(ArchiveStatus.Completed);
        task.ArchivePath.Should().Be(archivePath);
    }

    [Fact]
    public void SetCompleted_WithEmptyPath_ShouldThrow()
    {
        // Arrange
        var task = new ArchiveTask(Guid.NewGuid(), new[] { "file1.txt" });

        // Act
        Action act = () => task.SetCompleted("");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Archive path cannot be empty*");
    }

    [Fact]
    public void SetFailed_ShouldChangeStatusAndSetError()
    {
        // Arrange
        var task = new ArchiveTask(Guid.NewGuid(), new[] { "file1.txt" });
        var errorMessage = "Something went wrong";

        // Act
        task.SetFailed(errorMessage);

        // Assert
        task.Status.Should().Be(ArchiveStatus.Failed);
        task.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public void SetFailed_WithEmptyMessage_ShouldThrow()
    {
        // Arrange
        var task = new ArchiveTask(Guid.NewGuid(), new[] { "file1.txt" });

        // Act
        Action act = () => task.SetFailed("");

        // Assert
        act.Should().Throw<ArgumentException>().WithMessage("*Error message cannot be empty*");
    }
}