namespace DevDocs.Domain.ProjectDocumentations;

public class ProjectDocumentation
{
    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Title { get; private set; } = string.Empty;

    public string Overview { get; private set; } = string.Empty;

    public string Architecture { get; private set; } = string.Empty;

    public string MainFlows { get; private set; } = string.Empty;

    public string Technologies { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public string Generator { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    private ProjectDocumentation()
    {
    }

    public ProjectDocumentation(
        Guid projectId,
        string title,
        string overview,
        string architecture,
        string mainFlows,
        string technologies,
        string content,
        string generator)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Title = title;
        Overview = overview;
        Architecture = architecture;
        MainFlows = mainFlows;
        Technologies = technologies;
        Content = content;
        Generator = generator;
        CreatedAt = DateTime.UtcNow;
    }
}