using System;

namespace DevDocs.Application.Queue;

public record ProjectAnalysisMessage(Guid ProjectId, Guid JobId);
