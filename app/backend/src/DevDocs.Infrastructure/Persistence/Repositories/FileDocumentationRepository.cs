using DevDocs.Application.Abstractions;
using DevDocs.Domain.FileDocumentations;
using Microsoft.EntityFrameworkCore;

namespace DevDocs.Infrastructure.Persistence.Repositories;

public class FileDocumentationRepository : IFileDocumentationRepository
{
    private readonly DevDocsDbContext _dbContext;

    public FileDocumentationRepository(DevDocsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        FileDocumentation documentation,
        CancellationToken cancellationToken)
    {
        await _dbContext.FileDocumentations.AddAsync(
            documentation,
            cancellationToken
        );
    }

    public async Task<FileDocumentation?> GetBySourceFileIdAsync(
        Guid sourceFileId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.FileDocumentations
            .FirstOrDefaultAsync(
                documentation => documentation.SourceFileId == sourceFileId,
                cancellationToken
            );
    }

    public async Task<List<FileDocumentation>> GetByProjectIdAsync(
    Guid projectId,
    CancellationToken cancellationToken)
    {
        return await _dbContext.FileDocumentations
            .Where(documentation => documentation.ProjectId == projectId)
            .OrderByDescending(documentation => documentation.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteBySourceFileIdAsync(
        Guid sourceFileId,
        CancellationToken cancellationToken)
    {
        var documentation = await _dbContext.FileDocumentations
            .FirstOrDefaultAsync(
                documentation => documentation.SourceFileId == sourceFileId,
                cancellationToken
            );

        if (documentation is not null)
        {
            _dbContext.FileDocumentations.Remove(documentation);
        }
    }
}