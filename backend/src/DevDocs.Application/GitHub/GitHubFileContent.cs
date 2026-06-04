namespace DevDocs.Application.GitHub;

public sealed record GitHubFileContent(
    string Content,
    string Encoding,
    long Size
);
