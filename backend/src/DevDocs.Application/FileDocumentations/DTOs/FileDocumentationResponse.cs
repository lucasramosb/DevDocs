namespace DevDocs.Application.FileDocumentations.DTOs;

public sealed record FileDocumentationResponse(
    Guid Id,
    Guid ProjectId,
    Guid SourceFileId,
    string Summary,
    string Content,
    string Generator,
    DateTime CreatedAt
);
