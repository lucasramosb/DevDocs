namespace DevDocs.Domain.FileDocumentations;

public class FileDocumentation
{
    public Guid Id { get; private set; }

    public Guid SourceFileId { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Summary { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public string Generator { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    private FileDocumentation()
    {
    }

    public FileDocumentation(
        Guid sourceFileId,
        Guid projectId,
        string summary,
        string content,
        string generator)
    {
        Id = Guid.NewGuid();
        SourceFileId = sourceFileId;
        ProjectId = projectId;
        Summary = summary;
        Content = content;
        Generator = generator;
        CreatedAt = DateTime.UtcNow;
    }
}
