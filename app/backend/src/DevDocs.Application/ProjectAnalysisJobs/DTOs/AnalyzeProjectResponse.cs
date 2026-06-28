using System;

namespace DevDocs.Application.ProjectAnalysisJobs.DTOs;

public record AnalyzeProjectResponse(Guid ProjectId, Guid AnalysisJobId, string Status);
