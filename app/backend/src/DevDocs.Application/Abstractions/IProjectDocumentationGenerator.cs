namespace DevDocs.Application.Abstractions;

public interface IProjectDocumentationGenerator
{
    ProjectDocumentationResult Generate(ProjectDocumentationContext context);
}

public sealed record ProjectDocumentationContext(
    string ProjectName,
    string Owner,
    string RepositoryName,
    string GitHubUrl,
    string DefaultBranch,
    IReadOnlyList<ProjectFileDocumentationContext> Files
);

public sealed record ProjectFileDocumentationContext(
    Guid SourceFileId,
    string Path,
    string Extension,
    string Summary,
    string Generator
);

public sealed record ProjectDocumentationResult(
    string Title,
    string Overview,
    string Architecture,
    string MainFlows,
    string Technologies,
    string Content,
    string Generator
);