namespace DevDocs.Application.Abstractions;

public interface IFileDocumentationGenerator
{
    FileDocumentationResult Generate(
        string filePath,
        string extension,
        string content);
}

public sealed record FileDocumentationResult(
    string Summary,
    string Content,
    string Generator
);
