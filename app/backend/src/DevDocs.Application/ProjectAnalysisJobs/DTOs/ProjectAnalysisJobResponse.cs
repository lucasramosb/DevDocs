using System;

namespace DevDocs.Application.ProjectAnalysisJobs.DTOs;

public record ProjectAnalysisJobResponse(
    Guid Id,
    Guid ProjectId,
    string Status,
    string CurrentStep,
    int Progress,
    int FilesFound,
    int FilesProcessed,
    int FilesDocumented,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? FinishedAt
);
