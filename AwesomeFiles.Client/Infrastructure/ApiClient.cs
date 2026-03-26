using System.Net.Http.Json;
using System.Text.Json;
using AwesomeFiles.Client.Models;

namespace AwesomeFiles.Client.Infrastructure;
public class ApiClient : IApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string[]> GetFilesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("api/files", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);
        
        return await response.Content.ReadFromJsonAsync<string[]>(cancellationToken: cancellationToken) 
               ?? Array.Empty<string>();
    }

    public async Task<CreateArchiveResponse> CreateArchiveAsync(string[] fileNames, CancellationToken cancellationToken = default)
    {
        var request = new { fileNames };
        var response = await _httpClient.PostAsJsonAsync("api/archives", request, cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<CreateArchiveResponse>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("Empty response from server");
    }

    public async Task<StatusResponse> GetStatusAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync($"api/archives/{taskId}", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<StatusResponse>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException("Empty response from server");
    }

    public async Task DownloadArchiveAsync(Guid taskId, string destinationFolder, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(destinationFolder))
        {
            destinationFolder = Directory.GetCurrentDirectory();
        }
        
        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }

        var response = await _httpClient.GetAsync($"api/archives/{taskId}/download", cancellationToken);
        await EnsureSuccessAsync(response, cancellationToken);

        var fileName = $"{taskId}.zip";
        var filePath = Path.Combine(destinationFolder, fileName);
        
        await using var fileStream = new FileStream(filePath, FileMode.Create);
        await response.Content.CopyToAsync(fileStream, cancellationToken);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
            return;

        string errorMessage;
        try
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            try
            {
                var json = JsonDocument.Parse(errorContent);
                if (json.RootElement.TryGetProperty("detail", out var detail))
                {
                    errorMessage = detail.GetString() ?? errorContent;
                }
                else if (json.RootElement.TryGetProperty("error", out var error))
                {
                    errorMessage = error.GetString() ?? errorContent;
                }
                else
                {
                    errorMessage = errorContent;
                }
            }
            catch (JsonException)
            {
                errorMessage = errorContent;
            }
        }
        catch
        {
            errorMessage = response.ReasonPhrase ?? "Unknown error";
        }

        throw new HttpRequestException(
            $"Backend error ({(int)response.StatusCode}): {errorMessage}",
            null,
            response.StatusCode);
    }
}