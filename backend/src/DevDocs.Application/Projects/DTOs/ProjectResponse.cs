namespace DevDocs.Application.Projects.DTOs;

public sealed record ProjectResponse(
    Guid Id,
    string Name,
    string RepositoryPath,
    string? Description,
    DateTime CreatedAt
);