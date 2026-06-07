namespace DevDocs.Application.ProjectDocumentations.DTOs;

public sealed record ProjectDocumentationResponse(
    Guid Id,
    Guid ProjectId,
    string Title,
    string Overview,
    string Architecture,
    string MainFlows,
    string Technologies,
    string Content,
    string Generator,
    DateTime CreatedAt
);