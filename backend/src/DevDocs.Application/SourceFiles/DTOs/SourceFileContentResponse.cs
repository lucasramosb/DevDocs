namespace DevDocs.Application.SourceFiles.DTOs;

public sealed record SourceFileContentResponse(
    Guid SourceFileId,
    Guid ProjectId,
    string Path,
    string Extension,
    string Content
);
