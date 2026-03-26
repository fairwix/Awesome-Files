namespace AwesomeFiles.Application.DTOs;

public record ArchiveStatusResponse(Guid Id, string Status, string? Error = null);