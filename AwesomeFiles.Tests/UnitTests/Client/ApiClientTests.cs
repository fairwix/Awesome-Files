using System.Net;
using System.Text;
using System.Text.Json;
using AwesomeFiles.Client.Infrastructure;
using AwesomeFiles.Client.Models;
using FluentAssertions;
using Moq;
using Moq.Protected;

namespace AwesomeFiles.Tests.UnitTests.Client;

public class ApiClientTests
{
    private const string BaseUrl = "http://localhost:5083";

    private static Mock<HttpMessageHandler> CreateMockHandler(HttpStatusCode statusCode, string content)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = statusCode,
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            });
        return handlerMock;
    }

    [Fact]
    public async Task GetFilesAsync_ShouldReturnFileList()
    {
        var expectedFiles = new[] { "file1.txt", "file2.txt" };
        var json = JsonSerializer.Serialize(expectedFiles);
        var handlerMock = CreateMockHandler(HttpStatusCode.OK, json);
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri(BaseUrl) };
        var client = new ApiClient(httpClient);
        
        var result = await client.GetFilesAsync();
        
        result.Should().BeEquivalentTo(expectedFiles);
    }

    [Fact]
    public async Task CreateArchiveAsync_ShouldReturnResponse()
    {
        var expectedId = Guid.NewGuid();
        var responseObj = new CreateArchiveResponse(expectedId);
        var json = JsonSerializer.Serialize(responseObj);
        var handlerMock = CreateMockHandler(HttpStatusCode.Accepted, json);
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri(BaseUrl) };
        var client = new ApiClient(httpClient);
        
        var result = await client.CreateArchiveAsync(new[] { "file1.txt" });
        
        result.Id.Should().Be(expectedId);
    }

    [Fact]
    public async Task GetStatusAsync_ShouldReturnStatus()
    {
        var id = Guid.NewGuid();
        var statusObj = new StatusResponse(id, "Completed", null);
        var json = JsonSerializer.Serialize(statusObj);
        var handlerMock = CreateMockHandler(HttpStatusCode.OK, json);
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri(BaseUrl) };
        var client = new ApiClient(httpClient);
        
        var result = await client.GetStatusAsync(id);
        
        result.Id.Should().Be(id);
        result.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task DownloadArchiveAsync_ShouldSaveFile()
    {
        var id = Guid.NewGuid();
        var content = "dummy zip content";
        var handlerMock = CreateMockHandler(HttpStatusCode.OK, content);
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri(BaseUrl) };
        var client = new ApiClient(httpClient);
        var tempFolder = Path.GetTempPath();
        var expectedPath = Path.Combine(tempFolder, $"{id}.zip");
        
        await client.DownloadArchiveAsync(id, tempFolder);
        
        File.Exists(expectedPath).Should().BeTrue();
        
        File.Delete(expectedPath);
    }

    [Fact]
    public async Task CreateArchiveAsync_WhenBackendError_ShouldThrow()
    {
        var errorContent = "{\"detail\":\"File not found\"}";
        var handlerMock = CreateMockHandler(HttpStatusCode.BadRequest, errorContent);
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri(BaseUrl) };
        var client = new ApiClient(httpClient);
        
        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            () => client.CreateArchiveAsync(new[] { "nonexistent.txt" }));
        
        exception.Message.Should().Contain("400");
        exception.Message.Should().Contain("File not found");
    }
}