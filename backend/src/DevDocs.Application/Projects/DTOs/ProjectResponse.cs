namespace DevDocs.Application.Projects.DTOs;

public sealed record ProjectResponse(
    Guid Id,
    string Name,
    string Owner,
    string RepositoryName, 
    string GitHubUrl,
    string DefaultBranch,
    string? Description,
    DateTime CreateAt
);