using System.Net.Http.Json;
using System.Text.Json.Serialization;
using DevDocs.Application.Abstractions;
using DevDocs.Infrastructure.Ollama;
using Microsoft.Extensions.Options;

namespace DevDocs.Infrastructure.Documentation;

public class OllamaFileDocumentationGenerator : IFileDocumentationGenerator
{
    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;

    public OllamaFileDocumentationGenerator(
        HttpClient httpClient,
        IOptions<OllamaOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<FileDocumentationResult> GenerateAsync(
        string filePath,
        string extension,
        string content,
        CancellationToken cancellationToken)
    {
        var prompt = BuildPrompt(filePath, extension, content);

        var request = new OllamaGenerateRequest(
            _options.Model,
            prompt,
            false
        );

        var response = await _httpClient.PostAsJsonAsync(
            "/api/generate",
            request,
            cancellationToken
        );

        response.EnsureSuccessStatusCode();

        var ollamaResponse = await response.Content
            .ReadFromJsonAsync<OllamaGenerateResponse>(
                cancellationToken: cancellationToken
            );

        var generatedContent = ollamaResponse?.Response?.Trim();

        if (string.IsNullOrWhiteSpace(generatedContent))
        {
            generatedContent = "Não foi possível gerar documentação para este arquivo.";
        }

        var summary = ExtractSummary(generatedContent, filePath);

        return new FileDocumentationResult(
            summary,
            generatedContent,
            $"Ollama:{_options.Model}"
        );
    }

    private static string BuildPrompt(
        string filePath,
        string extension,
        string content)
    {
        var safeContent = LimitContent(content, 12_000);

        return $"""
        Você é um assistente técnico especializado em documentação de software.

        Gere uma documentação clara, objetiva e útil para o arquivo abaixo.

        Responda em português do Brasil.

        A documentação deve seguir exatamente este formato:

        # Documentação do arquivo

        ## Caminho

        Informe o caminho do arquivo.

        ## Tipo do arquivo

        Explique o tipo do arquivo.

        ## Responsabilidade principal

        Explique a responsabilidade principal deste arquivo.

        ## Como este arquivo funciona

        Explique os principais blocos, classes, métodos, endpoints, configurações ou responsabilidades.

        ## Dependências e integrações

        Liste dependências, serviços, bibliotecas, camadas ou integrações usadas.

        ## Pontos de atenção

        Liste possíveis riscos, acoplamentos, regras importantes ou melhorias futuras.

        ## Resumo curto

        Escreva um resumo final em 1 ou 2 frases.

        Dados do arquivo:

        Caminho: {filePath}
        Extensão: {extension}

        Conteúdo:

        ```txt
        {safeContent}
        ```
        """;
    }

    private static string LimitContent(string content, int maxLength)
    {
        if (content.Length <= maxLength)
        {
            return content;
        }

        return content[..maxLength] + """

        ... conteúdo truncado para não exceder o limite do modelo ...
        """;
    }

    private static string ExtractSummary(string generatedContent, string filePath)
    {
        const string summaryMarker = "## Resumo curto";

        var markerIndex = generatedContent.IndexOf(
            summaryMarker,
            StringComparison.OrdinalIgnoreCase
        );

        if (markerIndex < 0)
        {
            return $"Documentação gerada para o arquivo {filePath}.";
        }

        var summary = generatedContent[(markerIndex + summaryMarker.Length)..]
            .Trim()
            .Split('\n')
            .Select(line => line.Trim())
            .FirstOrDefault(line => !string.IsNullOrWhiteSpace(line));

        if (string.IsNullOrWhiteSpace(summary))
        {
            return $"Documentação gerada para o arquivo {filePath}.";
        }

        return summary;
    }

    private sealed record OllamaGenerateRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("prompt")] string Prompt,
        [property: JsonPropertyName("stream")] bool Stream
    );

    private sealed class OllamaGenerateResponse
    {
        [JsonPropertyName("response")]
        public string? Response { get; set; }
    }
}