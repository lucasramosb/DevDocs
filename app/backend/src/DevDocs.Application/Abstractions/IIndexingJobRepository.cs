using DevDocs.Domain.IndexingJobs;

namespace DevDocs.Application.Abstractions;

public interface IIndexingJobRepository
{
    Task AddAsync(
        IndexingJob indexingJob,
        CancellationToken cancellationToken);

    Task<IndexingJob?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<List<IndexingJob>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken);
}
