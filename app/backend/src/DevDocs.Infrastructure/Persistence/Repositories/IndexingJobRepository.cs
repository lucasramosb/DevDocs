using DevDocs.Application.Abstractions;
using DevDocs.Domain.IndexingJobs;
using Microsoft.EntityFrameworkCore;

namespace DevDocs.Infrastructure.Persistence.Repositories;

public class IndexingJobRepository : IIndexingJobRepository
{
    private readonly DevDocsDbContext _dbContext;

    public IndexingJobRepository(DevDocsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        IndexingJob indexingJob,
        CancellationToken cancellationToken)
    {
        await _dbContext.IndexingJobs.AddAsync(indexingJob, cancellationToken);
    }

    public async Task<IndexingJob?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return await _dbContext.IndexingJobs
            .FirstOrDefaultAsync(indexingJob => indexingJob.Id == id, cancellationToken);
    }

    public async Task<List<IndexingJob>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.IndexingJobs
            .Where(indexingJob => indexingJob.ProjectId == projectId)
            .OrderByDescending(indexingJob => indexingJob.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
