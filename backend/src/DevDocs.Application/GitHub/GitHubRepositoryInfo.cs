namespace DevDocs.Application.GitHub;

public sealed record GitHubRepositoryInfo(
    string Name,
    string Owner,
    string HtmlUrl,
    string DefaultBranch,
    string? Description,
    bool IsPrivate,
    bool IsArchived,
    bool IsFork
);
