namespace DevDocs.Application.FileDocumentations.DTOs;

public sealed record QueueProjectFilesDocumentationResponse(
    Guid ProjectId,
    int TotalFilesFound,
    int TotalFilesQueued,
    int TotalFilesSkipped,
    string Status,
    string Message
);