namespace DevDocs.Application.Abstractions;

public interface IFileDocumentationGenerator
{
    Task<FileDocumentationResult> GenerateAsync(
        string filePath,
        string extension,
        string content,
        CancellationToken cancellationToken);
}

public sealed record FileDocumentationResult(
    string Summary,
    string Content,
    string Generator
);