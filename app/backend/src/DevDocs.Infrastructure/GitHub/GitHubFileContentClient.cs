using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using DevDocs.Application.Abstractions;
using DevDocs.Application.GitHub;

namespace DevDocs.Infrastructure.GitHub;

public sealed class GitHubFileContentClient : IGitHubFileContentClient
{
    private readonly HttpClient _httpClient;

    public GitHubFileContentClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GitHubFileContent?> GetFileContentAsync(
        string owner,
        string repositoryName,
        string fileSha,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(
            $"/repos/{owner}/{repositoryName}/git/blobs/{fileSha}",
            cancellationToken
        );

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var blob = await response.Content
            .ReadFromJsonAsync<GitHubBlobResponse>(
                cancellationToken: cancellationToken
            );

        if (blob is null)
        {
            return null;
        }

        if (!string.Equals(blob.Encoding, "base64", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var normalizedContent = blob.Content
            .Replace("\n", string.Empty)
            .Replace("\r", string.Empty);

        var bytes = Convert.FromBase64String(normalizedContent);
        var content = Encoding.UTF8.GetString(bytes);

        return new GitHubFileContent(
            content,
            blob.Encoding,
            blob.Size
        );
    }

    private sealed class GitHubBlobResponse
    {
        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;

        [JsonPropertyName("encoding")]
        public string Encoding { get; set; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; set; }
    }
}
