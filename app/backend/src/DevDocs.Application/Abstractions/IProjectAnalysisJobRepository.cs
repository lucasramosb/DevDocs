using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DevDocs.Domain.ProjectAnalysisJobs;

namespace DevDocs.Application.Abstractions;

public interface IProjectAnalysisJobRepository
{
    Task AddAsync(ProjectAnalysisJob job, CancellationToken cancellationToken = default);
    Task UpdateAsync(ProjectAnalysisJob job, CancellationToken cancellationToken = default);
    Task<ProjectAnalysisJob?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProjectAnalysisJob>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
}
