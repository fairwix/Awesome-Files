using System.Text.Json;
using AwesomeFiles.Api.Middleware;
using AwesomeFiles.Application.Exceptions;
using AwesomeFiles.Domain.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using ApplicationException = AwesomeFiles.Application.Exceptions.ApplicationException;

namespace AwesomeFiles.Tests.UnitTests.Api;

public class ErrorHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ErrorHandlingMiddleware>> _loggerMock;
    private readonly ErrorHandlingMiddleware _middleware;

    public ErrorHandlingMiddlewareTests()
    {
        _loggerMock = new Mock<ILogger<ErrorHandlingMiddleware>>();
        _middleware = new ErrorHandlingMiddleware(next: (innerHttpContext) => Task.CompletedTask, _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_WhenApplicationExceptionThrown_ShouldReturn400()
    {
        var context = new DefaultHttpContext();
        var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;
        
        var middleware = new ErrorHandlingMiddleware(
            next: (innerHttpContext) => throw new ApplicationException("Test error"),
            _loggerMock.Object);
        
        await middleware.InvokeAsync(context);
        
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
        var json = JsonDocument.Parse(responseBody);
        json.RootElement.GetProperty("detail").GetString().Should().Be("Test error");
    }

    [Fact]
    public async Task InvokeAsync_WhenDomainExceptionThrown_ShouldReturn400()
    {
        var context = new DefaultHttpContext();
        var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;
        
        var middleware = new ErrorHandlingMiddleware(
            next: (innerHttpContext) => throw new DomainException("Domain error"),
            _loggerMock.Object);
        
        await middleware.InvokeAsync(context);
        
        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
        var json = JsonDocument.Parse(responseBody);
        json.RootElement.GetProperty("detail").GetString().Should().Be("Domain error");
    }

    [Fact]
    public async Task InvokeAsync_WhenFileNotFoundExceptionThrown_ShouldReturn404()
    {
        var context = new DefaultHttpContext();
        var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;
        
        var middleware = new ErrorHandlingMiddleware(
            next: (innerHttpContext) => throw new FileNotFoundException("File not found"),
            _loggerMock.Object);
        
        await middleware.InvokeAsync(context);
        
        context.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
        var json = JsonDocument.Parse(responseBody);
        json.RootElement.GetProperty("detail").GetString().Should().Be("File not found");
    }

    [Fact]
    public async Task InvokeAsync_WhenOperationCanceledExceptionThrown_ShouldReturn499()
    {
        var context = new DefaultHttpContext();
        var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;
        
        var middleware = new ErrorHandlingMiddleware(
            next: (innerHttpContext) => throw new OperationCanceledException(),
            _loggerMock.Object);
        
        await middleware.InvokeAsync(context);
        
        context.Response.StatusCode.Should().Be(StatusCodes.Status499ClientClosedRequest);
        
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
        var json = JsonDocument.Parse(responseBody);
        json.RootElement.GetProperty("detail").GetString().Should().Be("Request cancelled");
    }

    [Fact]
    public async Task InvokeAsync_WhenGenericExceptionThrown_ShouldReturn500()
    {
        var context = new DefaultHttpContext();
        var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;
        
        var middleware = new ErrorHandlingMiddleware(
            next: (innerHttpContext) => throw new Exception("Unexpected error"),
            _loggerMock.Object);
        
        await middleware.InvokeAsync(context);
        
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
        var json = JsonDocument.Parse(responseBody);
        json.RootElement.GetProperty("detail").GetString().Should().Be("Internal server error");
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_ShouldCallNext()
    {
        var context = new DefaultHttpContext();
        var nextCalled = false;
        
        var middleware = new ErrorHandlingMiddleware(
            next: (innerHttpContext) => { nextCalled = true; return Task.CompletedTask; },
            _loggerMock.Object);
        
        await middleware.InvokeAsync(context);
        
        nextCalled.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}