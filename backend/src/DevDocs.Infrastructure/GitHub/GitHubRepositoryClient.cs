using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DevDocs.Application.Abstractions;
using DevDocs.Application.GitHub;

namespace DevDocs.Infrastructure.GitHub;

public sealed class GitHubRepositoryClient : IGitHubRepositoryClient
{
    private readonly HttpClient _httpClient;

    public GitHubRepositoryClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<GitHubRepositoryInfo?> GetPublicRepositoryAsync(
        string owner,
        string repositoryName,
        CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetAsync(
            $"/repos/{owner}/{repositoryName}",
            cancellationToken
        );

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var repository = await response.Content
            .ReadFromJsonAsync<GitHubRepositoryResponse>(
                cancellationToken: cancellationToken
            );

        if (repository is null)
        {
            return null;
        }

        if (repository.Private)
        {
            return null;
        }

        return new GitHubRepositoryInfo(
            repository.Name,
            repository.Owner.Login,
            repository.HtmlUrl,
            repository.DefaultBranch,
            repository.Description,
            repository.Private,
            repository.Archived,
            repository.Fork
        );
    }

    private sealed class GitHubRepositoryResponse
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("html_url")]
        public string HtmlUrl { get; set; } = string.Empty;

        [JsonPropertyName("default_branch")]
        public string DefaultBranch { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("private")]
        public bool Private { get; set; }

        [JsonPropertyName("archived")]
        public bool Archived { get; set; }

        [JsonPropertyName("fork")]
        public bool Fork { get; set; }

        [JsonPropertyName("owner")]
        public GitHubOwnerResponse Owner { get; set; } = new();
    }

    private sealed class GitHubOwnerResponse
    {
        [JsonPropertyName("login")]
        public string Login { get; set; } = string.Empty;
    }
}