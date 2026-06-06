using DevDocs.Domain.Projects;

namespace DevDocs.Application.Abstractions;

public interface IProjectRepository
{
    Task AddAsync(Project project, CancellationToken cancellationToken);

    Task<List<Project>> GetAllAsync(CancellationToken cancellationToken);

    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
