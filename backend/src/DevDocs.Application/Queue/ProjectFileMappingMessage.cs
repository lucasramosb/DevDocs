namespace DevDocs.Application.Queue;

public sealed record ProjectFileMappingMessage(
    Guid ProjectId,
    Guid IndexingJobId
);
