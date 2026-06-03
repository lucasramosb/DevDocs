namespace DevDocs.Application.SourceFiles.DTOs;

public sealed record SourceFileResponse(
    Guid Id,
    Guid ProjectId,
    string Path,
    string Name,
    string Extension,
    long Size,
    bool IsDocumentationFile,
    bool IsTestFile,
    DateTime CreatedAt
);
