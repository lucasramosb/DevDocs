using DevDocs.Application.Abstractions;
using DevDocs.Domain.SourceFiles;
using Microsoft.EntityFrameworkCore;

namespace DevDocs.Infrastructure.Persistence.Repositories;

public class SourceFileRepository : ISourceFileRepository
{
    private readonly DevDocsDbContext _dbContext;

    public SourceFileRepository(DevDocsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddRangeAsync(
        List<SourceFile> sourceFiles,
        CancellationToken cancellationToken)
    {
        await _dbContext.SourceFiles.AddRangeAsync(sourceFiles, cancellationToken);
    }

    public async Task<List<SourceFile>> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.SourceFiles
            .Where(sourceFile => sourceFile.ProjectId == projectId)
            .OrderBy(sourceFile => sourceFile.Path)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var sourceFiles = await _dbContext.SourceFiles
            .Where(sourceFile => sourceFile.ProjectId == projectId)
            .ToListAsync(cancellationToken);

        _dbContext.SourceFiles.RemoveRange(sourceFiles);
    }

    public async Task<SourceFile?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        return await _dbContext.SourceFiles
        .FirstOrDefaultAsync(SourceFile => SourceFile.Id == id, cancellationToken);
    }
}
