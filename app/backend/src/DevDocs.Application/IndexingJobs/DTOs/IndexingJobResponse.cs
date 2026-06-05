namespace DevDocs.Application.IndexingJobs.DTOs;

public sealed record IndexingJobResponse
(
    Guid Id,
    Guid ProjectId,
    string Status,
    int TotalFilesFound,
    int TotalFilesMapped,
    int TotalFilesIgnored,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? FinishedAt
);
