namespace DevDocs.Application.FileDocumentations.DTOs;

public sealed record QueueFileDocumentationResponse(
    Guid ProjectId,
    Guid SourceFileId,
    string Status,
    string Message
);