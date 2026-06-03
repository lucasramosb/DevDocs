using DevDocs.Application.GitHub;

namespace DevDocs.Application.Abstractions;

public interface IGitHubRepositoryFileClient
{
    Task<List<GitHubRepositoryFile>> GetRepositoryFilesAsync(
        string owner,
        string repositoryName,
        string branch,
        CancellationToken cancellationToken);
}
