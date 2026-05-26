namespace DevDocs.Domain.Projects;

public class Project
{
    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public string RepositoryPath { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private Project()
    {
        
    }

    public Project(string name, string repositoryPath, string? description = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        RepositoryPath = repositoryPath;
        Description = description;
        CreatedAt = DateTime.UtcNow;
    }
}