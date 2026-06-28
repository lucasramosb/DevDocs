using DevDocs.Application.Abstractions;
using DevDocs.Domain.Projects;
using Microsoft.EntityFrameworkCore;

namespace DevDocs.Infrastructure.Persistence.Repositories;

public class ProjectRepository : IProjectRepository
{
    private readonly DevDocsDbContext _dbContext;

    public ProjectRepository(DevDocsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Project project, CancellationToken cancellationToken)
    {
        await _dbContext.Projects.AddAsync(project, cancellationToken);
    }

    public async Task<List<Project>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Projects
            .OrderByDescending(project => project.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Projects
            .FirstOrDefaultAsync(project => project.Id == id, cancellationToken);
    }

    public async Task<Project?> GetByGitHubUrlAsync(string githubUrl, CancellationToken cancellationToken)
    {
        return await _dbContext.Projects
            .FirstOrDefaultAsync(project => project.GitHubUrl == githubUrl, cancellationToken);
    }
}
