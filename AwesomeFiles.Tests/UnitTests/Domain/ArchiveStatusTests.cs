using AwesomeFiles.Domain.Enums;
using FluentAssertions;

namespace AwesomeFiles.Tests.UnitTests.Domain;

public class ArchiveStatusTests
{
    [Fact]
    public void ArchiveStatus_ShouldHaveFourValues()
    {
        Enum.GetValues<ArchiveStatus>().Should().HaveCount(4);
    }

    [Fact]
    public void ArchiveStatus_Values_ShouldBeCorrect()
    {
        ((int)ArchiveStatus.Pending).Should().Be(0);
        ((int)ArchiveStatus.InProgress).Should().Be(1);
        ((int)ArchiveStatus.Completed).Should().Be(2);
        ((int)ArchiveStatus.Failed).Should().Be(3);
    }

    [Fact]
    public void ArchiveStatus_Names_ShouldBeCorrect()
    {
        ArchiveStatus.Pending.ToString().Should().Be("Pending");
        ArchiveStatus.InProgress.ToString().Should().Be("InProgress");
        ArchiveStatus.Completed.ToString().Should().Be("Completed");
        ArchiveStatus.Failed.ToString().Should().Be("Failed");
    }
}