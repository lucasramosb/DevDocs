using DevDocs.Application.Queue;

namespace DevDocs.Application.Abstractions;

public interface IFileDocumentationGenerationQueue
{
    Task EnqueueAsync(
        FileDocumentationGenerationMessage message,
        CancellationToken cancellationToken);

    Task<FileDocumentationGenerationMessage?> DequeueAsync(
        CancellationToken cancellationToken);
}