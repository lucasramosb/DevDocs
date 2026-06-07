using DevDocs.Domain.ProjectDocumentations;

namespace DevDocs.Application.Abstractions;

public interface IProjectDocumentationRepository
{
    Task AddAsync(
        ProjectDocumentation documentation,
        CancellationToken cancellationToken);

    Task<ProjectDocumentation?> GetByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken);

    Task DeleteByProjectIdAsync(
        Guid projectId,
        CancellationToken cancellationToken);
}