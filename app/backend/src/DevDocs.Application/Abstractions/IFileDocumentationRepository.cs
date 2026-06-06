using DevDocs.Domain.FileDocumentations;

namespace DevDocs.Application.Abstractions;

public interface IFileDocumentationRepository
{
    Task AddAsync(
        FileDocumentation documentation,
        CancellationToken cancellationToken);

    Task<FileDocumentation?> GetBySourceFileIdAsync(
        Guid sourceFileId,
        CancellationToken cancellationToken);

    Task DeleteBySourceFileIdAsync(
        Guid sourceFileId,
        CancellationToken cancellationToken);
}
