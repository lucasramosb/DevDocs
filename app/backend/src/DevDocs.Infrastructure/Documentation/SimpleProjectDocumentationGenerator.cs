using DevDocs.Application.Abstractions;

namespace DevDocs.Infrastructure.Documentation;

public class SimpleProjectDocumentationGenerator : IProjectDocumentationGenerator
{
    public ProjectDocumentationResult Generate(ProjectDocumentationContext context)
    {
        var title = $"Documentação do projeto {context.ProjectName}";

        var technologies = DetectTechnologies(context.Files);

        var overview = BuildOverview(context);

        var architecture = BuildArchitecture(context);

        var mainFlows = BuildMainFlows(context);

        var content = $"""
        # {title}

        ## Visão Geral

        {overview}

        ## Repositório

        - Owner: `{context.Owner}`
        - Nome: `{context.RepositoryName}`
        - URL: `{context.GitHubUrl}`
        - Branch padrão: `{context.DefaultBranch}`

        ## Tecnologias Detectadas

        {technologies}

        ## Arquitetura Identificada

        {architecture}

        ## Fluxos Principais

        {mainFlows}

        ## Arquivos Documentados

        {BuildDocumentedFilesSection(context.Files)}

        ## Observação

        Esta documentação foi gerada por um gerador simples baseado em regras.
        Futuramente esta etapa será substituída por IA local com Ollama.
        """;

        return new ProjectDocumentationResult(
            title,
            overview,
            architecture,
            mainFlows,
            technologies,
            content,
            "SimpleProjectDocumentationGenerator"
        );
    }

    private static string BuildOverview(ProjectDocumentationContext context)
    {
        var totalFiles = context.Files.Count;

        if (totalFiles == 0)
        {
            return "Ainda não existem documentações de arquivos suficientes para gerar uma visão geral detalhada.";
        }

        return
            $"O projeto `{context.ProjectName}` possui {totalFiles} arquivos documentados. " +
            "A documentação geral foi construída a partir dos resumos gerados para os arquivos individuais.";
    }

    private static string BuildArchitecture(ProjectDocumentationContext context)
    {
        var topLevelFolders = context.Files
            .Select(file => file.Path.Split('/').FirstOrDefault())
            .Where(folder => !string.IsNullOrWhiteSpace(folder))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(folder => folder)
            .ToList();

        if (topLevelFolders.Count == 0)
        {
            return "Não foi possível identificar uma estrutura de pastas relevante.";
        }

        var folders = string.Join(
            "\n",
            topLevelFolders.Select(folder => $"- `{folder}`")
        );

        return $"""
        A estrutura do projeto indica organização nos seguintes diretórios principais:

        {folders}
        """;
    }

    private static string BuildMainFlows(ProjectDocumentationContext context)
    {
        var relevantFiles = context.Files
            .Where(file =>
                file.Path.Contains("Program.cs", StringComparison.OrdinalIgnoreCase) ||
                file.Path.Contains("Endpoint", StringComparison.OrdinalIgnoreCase) ||
                file.Path.Contains("Controller", StringComparison.OrdinalIgnoreCase) ||
                file.Path.Contains("Worker", StringComparison.OrdinalIgnoreCase) ||
                file.Path.Contains("Service", StringComparison.OrdinalIgnoreCase))
            .Take(10)
            .ToList();

        if (relevantFiles.Count == 0)
        {
            return "Nenhum fluxo principal foi identificado automaticamente nesta versão.";
        }

        var lines = relevantFiles
            .Select(file => $"- `{file.Path}`: {file.Summary}");

        return string.Join("\n", lines);
    }

    private static string DetectTechnologies(
        IReadOnlyList<ProjectFileDocumentationContext> files)
    {
        var technologies = new List<string>();

        if (files.Any(file => file.Extension.Equals(".cs", StringComparison.OrdinalIgnoreCase)))
        {
            technologies.Add(".NET / C#");
        }

        if (files.Any(file => file.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase)))
        {
            technologies.Add("Projetos .NET");
        }

        if (files.Any(file => file.Extension.Equals(".sln", StringComparison.OrdinalIgnoreCase)))
        {
            technologies.Add("Solution .NET");
        }

        if (files.Any(file => file.Extension.Equals(".json", StringComparison.OrdinalIgnoreCase)))
        {
            technologies.Add("JSON");
        }

        if (files.Any(file =>
            file.Extension.Equals(".yml", StringComparison.OrdinalIgnoreCase) ||
            file.Extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase)))
        {
            technologies.Add("YAML");
        }

        if (files.Any(file => file.Extension.Equals(".md", StringComparison.OrdinalIgnoreCase)))
        {
            technologies.Add("Markdown");
        }

        if (technologies.Count == 0)
        {
            return "Nenhuma tecnologia foi identificada automaticamente.";
        }

        return string.Join(", ", technologies.Distinct());
    }

    private static string BuildDocumentedFilesSection(
        IReadOnlyList<ProjectFileDocumentationContext> files)
    {
        if (files.Count == 0)
        {
            return "Nenhum arquivo documentado até o momento.";
        }

        var lines = files
            .OrderBy(file => file.Path)
            .Take(50)
            .Select(file => $"- `{file.Path}`: {file.Summary}");

        return string.Join("\n", lines);
    }
}