namespace AwesomeFiles.Client.Models;
public record StatusResponse(Guid Id, string Status, string? Error = null);