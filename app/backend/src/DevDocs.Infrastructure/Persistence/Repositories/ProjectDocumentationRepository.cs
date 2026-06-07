using DevDocs.Application.Abstractions;
using DevDocs.Domain.ProjectDocumentations;
using Microsoft.EntityFrameworkCore;

namespace DevDocs.Infrastructure.Persistence.Repositories;

public class ProjectDocumentationRepository : IProjectDocumentationRepository
{
    private readonly DevDocsDbContext _dbContext;

    public ProjectDocumentationRepository(DevDocsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        ProjectDocumentation documentation,
        CancellationToken cancellationToken)
    {
        await _dbContext.ProjectDocumentations.AddAsync(
            documentation,
            cancellationToken
        );
    }

    public async Task<ProjectDocumentation?> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ProjectDocumentations
            .FirstOrDefaultAsync(
                documentation => documentation.ProjectId == projectId,
                cancellationToken
            );
    }

    public async Task DeleteByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var documentation = await _dbContext.ProjectDocumentations
            .FirstOrDefaultAsync(
                documentation => documentation.ProjectId == projectId,
                cancellationToken
            );

        if (documentation is not null)
        {
            _dbContext.ProjectDocumentations.Remove(documentation);
        }
    }
}