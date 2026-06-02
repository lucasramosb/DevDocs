namespace DevDocs.Domain.Projects;

public class Project
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string Owner { get; private set; } = string.Empty;

    public string RepositoryName { get; private set; } = string.Empty;

    public string GitHubUrl { get; private set; } = string.Empty;

    public string DefaultBranch { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Project()
    {
    }

    public Project(
        string name,
        string owner,
        string repositoryName,
        string gitHubUrl,
        string defaultBranch,
        string? description)
    {
        Id = Guid.NewGuid();
        Name = name;
        Owner = owner;
        RepositoryName = repositoryName;
        GitHubUrl = gitHubUrl;
        DefaultBranch = defaultBranch;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }
}