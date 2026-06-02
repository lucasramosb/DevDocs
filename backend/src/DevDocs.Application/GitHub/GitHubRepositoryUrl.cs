namespace DevDocs.Application.GitHub;

public sealed record GitHubRepositoryUrl(
    string Owner,
    string RepositoryName,
    string Url
)
{
    public static bool TryParse(string input, out GitHubRepositoryUrl? repositoryUrl)
    {
        repositoryUrl = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var normalizdInput = input.Trim();

        if(!Uri.TryCreate(normalizdInput, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!string.Equals(uri.Host, "github.com",StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var segments = uri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (segments.Length != 2)
        {
            return false;
        }

        var owner = segments[0];
        var repositoryName = segments[1].Replace(".git", "");

        if (string.IsNullOrWhiteSpace(owner) ||
            string.IsNullOrWhiteSpace(repositoryName))
        {
            return false;
        }

        var cleanUrl = $"https://github.com/{owner}/{repositoryName}";

        repositoryUrl = new GitHubRepositoryUrl(
            owner,
            repositoryName,
            cleanUrl
        );

        return true;
    }
}