namespace DevDocs.Application.Projects.DTOs;

public sealed record CreateProjectRequest(
    string Name,
    string RepositoryPath,
    string? Description
);