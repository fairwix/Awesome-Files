using AwesomeFiles.Domain.Exceptions;
using FluentAssertions;

namespace AwesomeFiles.Tests.UnitTests.Domain;

public class DomainExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_ShouldSetMessage()
    {
        var message = "Test error message";
        
        var exception = new DomainException(message);
        
        exception.Message.Should().Be(message);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_ShouldSetBoth()
    {
        var message = "Test error message";
        var innerException = new Exception("Inner exception");
        
        var exception = new DomainException(message, innerException);
        
        exception.Message.Should().Be(message);
        exception.InnerException.Should().Be(innerException);
    }

    [Fact]
    public void DomainException_ShouldBeOfTypeException()
    {
        var exception = new DomainException("test");
        
        exception.Should().BeAssignableTo<Exception>();
    }
    
}