using DevDocs.Application.Queue;

namespace DevDocs.Application.Abstractions;

public interface IProjectFileMappingQueue
{
    Task EnqueueAsync(
        ProjectFileMappingMessage message,
        CancellationToken cancellationToken
    );

    Task<ProjectFileMappingMessage?> DequeueAsync(
        CancellationToken cancellationToken
    );
}
