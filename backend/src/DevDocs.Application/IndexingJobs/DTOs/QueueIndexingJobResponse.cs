namespace DevDocs.Application.IndexingJobs.DTOs;

public sealed record QueueIndexingJobResponse(
    Guid ProjectId,
    Guid IndexingJobId,
    string Status,
    string Message
);
