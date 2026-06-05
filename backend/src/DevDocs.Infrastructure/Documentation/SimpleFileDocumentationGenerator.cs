using DevDocs.Application.Abstractions;

namespace DevDocs.Infrastructure.Documentation;

public class SimpleFileDocumentationGenerator : IFileDocumentationGenerator
{
    public FileDocumentationResult Generate(
        string filePath,
        string extension,
        string content)
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

        ## Observações iniciais

        Este arquivo foi analisado por um gerador simples baseado em regras.

        ## Primeiras linhas do arquivo

        ```txt
        {GetPreview(content)}
        ```
        """;

        return new FileDocumentationResult(
            summary,
            documentation,
            "SimpleRuleBasedGenerator"
        );
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
            ".ts" => "código TypeScript",
            ".tsx" => "código TypeScript React",
            ".js" => "código JavaScript",
            ".jsx" => "código JavaScript React",
            ".css" => "arquivo de estilos CSS",
            "json" => "arquivo JSON",
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
