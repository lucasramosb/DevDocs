using DevDocs.Application.GitHub;

namespace DevDocs.Application.Abstractions;

public interface IGitHubFileContentClient
{
    Task<GitHubFileContent?> GetFileContentAsync(
        string owner,
        string repositoryName,
        string fileSha,
        CancellationToken cancellationToken = default
    );
}
