using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DevDocs.Application.Abstractions;
using DevDocs.Domain.ProjectAnalysisJobs;
using Microsoft.EntityFrameworkCore;

namespace DevDocs.Infrastructure.Persistence.Repositories;

public class ProjectAnalysisJobRepository : IProjectAnalysisJobRepository
{
    private readonly DevDocsDbContext _dbContext;

    public ProjectAnalysisJobRepository(DevDocsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        ProjectAnalysisJob job,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ProjectAnalysisJobs.AddAsync(job, cancellationToken);
    }

    public Task UpdateAsync(
        ProjectAnalysisJob job,
        CancellationToken cancellationToken = default)
    {
        _dbContext.ProjectAnalysisJobs.Update(job);
        return Task.CompletedTask;
    }

    public async Task<ProjectAnalysisJob?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectAnalysisJobs
            .FirstOrDefaultAsync(job => job.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ProjectAnalysisJob>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectAnalysisJobs
            .Where(job => job.ProjectId == projectId)
            .OrderByDescending(job => job.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
