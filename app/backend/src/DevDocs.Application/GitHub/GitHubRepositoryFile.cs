namespace DevDocs.Application.GitHub;

public sealed record GitHubRepositoryFile(
    string Path,
    string Sha,
    string GitHubBlobUrl,
    long Size
);
