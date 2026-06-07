namespace DevDocs.Application.Queue;

public sealed record FileDocumentationGenerationMessage(
    Guid ProjectId,
    Guid SourceFileId
);