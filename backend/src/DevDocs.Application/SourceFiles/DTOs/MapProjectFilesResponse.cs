namespace DevDocs.Application.SourceFiles.DTOs;

public sealed record MapProjectFilesResponse(
    Guid ProjectId,
    int TotalFilesFound,
    int TotalFilesMapped,
    int TotalFilesIgnored
);
