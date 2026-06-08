using DevDocs.Application.Abstractions;

namespace DevDocs.Infrastructure.Documentation;

public class SimpleFileDocumentationGenerator : IFileDocumentationGenerator
{
    public Task<FileDocumentationResult> GenerateAsync(
        string filePath,
        string extension,
        string content,
        CancellationToken cancellationToken)
    {
        var fileType = GetFileType(extension);

        var summary = $"Arquivo {fileType} localizado em {filePath}.";

        var documentation = $"""
        # Documentação do arquivo

        ## Caminho

        `{filePath}`

        ## Tipo

        {fileType}

        ## Resumo

        {summary}

        ## Observações

        Esta documentação foi gerada por um gerador simples baseado em regras.

        ## Prévia do arquivo

        ```txt
        {GetPreview(content)}
        ```
        """;

        var result = new FileDocumentationResult(
            summary,
            documentation,
            "SimpleRuleBasedGenerator"
        );

        return Task.FromResult(result);
    }

    private static string GetFileType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".cs" => "código C#",
            ".csproj" => "arquivo de projeto .NET",
            ".sln" => "solution .NET",
            ".md" => "documentação Markdown",
            ".json" => "arquivo JSON",
            ".yml" => "arquivo YAML",
            ".yaml" => "arquivo YAML",
            _ => "arquivo de código ou configuração"
        };
    }

    private static string GetPreview(string content)
    {
        var lines = content
            .Split('\n')
            .Take(20)
            .Select(line => line.TrimEnd());

        return string.Join('\n', lines);
    }
}