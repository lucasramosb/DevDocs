using DevDocs.Application.Abstractions;
using DevDocs.Application.GitHub;
using DevDocs.Application.Projects.DTOs;
using DevDocs.Application.SourceFiles.DTOs;
using DevDocs.Domain.Projects;
using DevDocs.Domain.SourceFiles;

namespace DevDocs.Api.Endpoints;

public static class ProjectsEndpoints
{
    public static void MapProjectsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects")
            .WithTags("Projects");

        group.MapPost("/", async (
            CreateProjectRequest request,
            IGitHubRepositoryClient gitHubRepositoryClient,
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            if (!GitHubRepositoryUrl.TryParse(request.GitHubUrl, out var repositoryUrl) ||
                repositoryUrl is null)
            {
                return Results.BadRequest("Informe uma URL válida de repositório do GitHub.");
            }

            var repositoryInfo = await gitHubRepositoryClient.GetPublicRepositoryAsync(
                repositoryUrl.Owner,
                repositoryUrl.RepositoryName,
                cancellationToken
            );

            if (repositoryInfo is null)
            {
                return Results.BadRequest("Repositório não encontrado ou não é público.");
            }

            var project = new Project(
                repositoryInfo.Name,
                repositoryInfo.Owner,
                repositoryInfo.Name,
                repositoryInfo.HtmlUrl,
                repositoryInfo.DefaultBranch,
                repositoryInfo.Description
            );

            await projectRepository.AddAsync(project, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new ProjectResponse(
                project.Id,
                project.Name,
                project.Owner,
                project.RepositoryName,
                project.GitHubUrl,
                project.DefaultBranch,
                project.Description,
                project.CreatedAt
            );

            return Results.Created($"/projects/{project.Id}", response);
        });

        group.MapGet("/", async (
            IProjectRepository projectRepository,
            CancellationToken cancellationToken) =>
        {
            var projects = await projectRepository.GetAllAsync(cancellationToken);

            var response = projects
                .Select(project => new ProjectResponse(
                    project.Id,
                    project.Name,
                    project.Owner,
                    project.RepositoryName,
                    project.GitHubUrl,
                    project.DefaultBranch,
                    project.Description,
                    project.CreatedAt
                ))
                .ToList();

            return Results.Ok(response);
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            IProjectRepository projectRepository,
            CancellationToken cancellationToken) =>
        {
            var project = await projectRepository.GetByIdAsync(id, cancellationToken);

            if (project is null)
            {
                return Results.NotFound();
            }

            var response = new ProjectResponse(
                project.Id,
                project.Name,
                project.Owner,
                project.RepositoryName,
                project.GitHubUrl,
                project.DefaultBranch,
                project.Description,
                project.CreatedAt
            );

            return Results.Ok(response);
        });

        group.MapPost("/{id:guid}/map-files", async (
        Guid id,
        IProjectRepository projectRepository,
        ISourceFileRepository sourceFileRepository,
        IGitHubRepositoryFileClient gitHubRepositoryFileClient,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken) =>
    {
        var project = await projectRepository.GetByIdAsync(id, cancellationToken);

        if (project is null)
        {
            return Results.NotFound("Projeto não encontrado.");
        }

        var githubFiles = await gitHubRepositoryFileClient.GetRepositoryFilesAsync(
            project.Owner,
            project.RepositoryName,
            project.DefaultBranch,
            cancellationToken
        );

        var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs",
            ".csproj",
            ".sln",
            ".md",
            ".json",
            ".yml",
            ".yaml"
        };

        var mappedFiles = githubFiles
            .Where(file => allowedExtensions.Contains(Path.GetExtension(file.Path)))
            .Select(file =>
            {
                var extension = Path.GetExtension(file.Path);
                var name = Path.GetFileName(file.Path);

                var isDocumentationFile =
                    extension.Equals(".md", StringComparison.OrdinalIgnoreCase) ||
                    file.Path.StartsWith("docs/", StringComparison.OrdinalIgnoreCase);

                var isTestFile =
                    file.Path.Contains("test", StringComparison.OrdinalIgnoreCase) ||
                    file.Path.Contains("tests", StringComparison.OrdinalIgnoreCase);

                return new SourceFile(
                    project.Id,
                    file.Path,
                    name,
                    extension,
                    file.Sha,
                    file.GitHubBlobUrl,
                    file.Size,
                    isDocumentationFile,
                    isTestFile
                );
            })
            .ToList();

        await sourceFileRepository.DeleteByProjectIdAsync(project.Id, cancellationToken);
        await sourceFileRepository.AddRangeAsync(mappedFiles, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new MapProjectFilesResponse(
            project.Id,
            githubFiles.Count,
            mappedFiles.Count,
            githubFiles.Count - mappedFiles.Count
        );

        return Results.Ok(response);
    });

        group.MapGet("/{id:guid}/files", async (
            Guid id,
            IProjectRepository projectRepository,
            ISourceFileRepository sourceFileRepository,
            CancellationToken cancellationToken) =>
        {
            var project = await projectRepository.GetByIdAsync(id, cancellationToken);

            if (project is null)
            {
                return Results.NotFound("Projeto não encontrado.");
            }

            var sourceFiles = await sourceFileRepository.GetByProjectIdAsync(
                project.Id,
                cancellationToken
            );

            var response = sourceFiles
                .Select(sourceFile => new SourceFileResponse(
                    sourceFile.Id,
                    sourceFile.ProjectId,
                    sourceFile.Path,
                    sourceFile.Name,
                    sourceFile.Extension,
                    sourceFile.Size,
                    sourceFile.IsDocumentationFile,
                    sourceFile.IsTestFile,
                    sourceFile.CreatedAt
                ))
                .ToList();

            return Results.Ok(response);
        });

    }
}
