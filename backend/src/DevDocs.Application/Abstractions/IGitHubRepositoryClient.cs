using DevDocs.Application.GitHub;

namespace DevDocs.Application.Abstractions;

public interface IGitHubRepositoryClient
{
    Task<GitHubRepositoryInfo?> GetPublicRepositoryAsync(
        string owner,
        string repositoryName,
        CancellationToken cancellationToken);
}