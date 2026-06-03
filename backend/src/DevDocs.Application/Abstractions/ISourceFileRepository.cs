using DevDocs.Domain.SourceFiles;

namespace DevDocs.Application.Abstractions;

public interface ISourceFileRepository
{
    Task AddRangeAsync(
        List<SourceFile> sourceFiles,
        CancellationToken cancellationToken);

    Task<List<SourceFile>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken);

    Task DeleteByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken);
}
