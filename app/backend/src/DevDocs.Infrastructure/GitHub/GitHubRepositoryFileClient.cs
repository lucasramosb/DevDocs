using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DevDocs.Application.Abstractions;
using DevDocs.Application.GitHub;

namespace DevDocs.Infrastructure.GitHub;

public sealed class GitHubRepositoryFileClient : IGitHubRepositoryFileClient
{
    private readonly HttpClient _httpClient;

    public GitHubRepositoryFileClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<GitHubRepositoryFile>> GetRepositoryFilesAsync(
        string owner,
        string repositoryName,
        string branch,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(
            $"/repos/{owner}/{repositoryName}/git/trees/{branch}?recursive=1",
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        var treeResponse = await response.Content
            .ReadFromJsonAsync<GitHubTreeResponse>(
                cancellationToken: cancellationToken
            );

        if (treeResponse is null)
        {
            return [];
        }

        return treeResponse.Tree
            .Where(item => item.Type == "blob")
            .Where(item => !string.IsNullOrWhiteSpace(item.Path))
            .Select(item => new GitHubRepositoryFile(
                item.Path,
                item.Sha,
                item.Url,
                item.Size ?? 0
            ))
            .ToList();
    }

    private sealed class GitHubTreeResponse
    {
        [JsonPropertyName("tree")]
        public List<GitHubTreeItemResponse> Tree { get; set; } = [];
    }

    private sealed class GitHubTreeItemResponse
    {
        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("sha")]
        public string Sha { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public long? Size { get; set; }
    }
}
