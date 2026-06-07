using DevDocs.Application.Abstractions;
using DevDocs.Application.FileDocumentations.DTOs;
using DevDocs.Application.GitHub;
using DevDocs.Application.IndexingJobs.DTOs;
using DevDocs.Application.Projects.DTOs;
using DevDocs.Application.Queue;
using DevDocs.Application.SourceFiles.DTOs;
using DevDocs.Domain.IndexingJobs;
using DevDocs.Domain.Projects;

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
            IIndexingJobRepository indexingJobRepository,
            IProjectFileMappingQueue projectFileMappingQueue,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var project = await projectRepository.GetByIdAsync(id, cancellationToken);

            if (project is null)
            {
                return Results.NotFound("Projeto não encontrado.");
            }

            var indexingJob = new IndexingJob(project.Id);

            await indexingJobRepository.AddAsync(indexingJob, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var message = new ProjectFileMappingMessage(
                project.Id,
                indexingJob.Id
            );

            await projectFileMappingQueue.EnqueueAsync(message, cancellationToken);

            var response = new QueueIndexingJobResponse(
                project.Id,
                indexingJob.Id,
                indexingJob.Status.ToString(),
                "Mapeamento de arquivos enviado para processamento."
            );

            return Results.Accepted(
                $"/projects/{project.Id}/indexing-jobs/{indexingJob.Id}",
                response
            );
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

        group.MapGet("/{projectId:guid}/files/{sourceFileId:guid}/content", async (
            Guid projectId,
            Guid sourceFileId,
            IProjectRepository projectRepository,
            ISourceFileRepository sourceFileRepository,
            IGitHubFileContentClient gitHubFileContentClient,
            CancellationToken cancellationToken) =>
        {
            var project = await projectRepository.GetByIdAsync(projectId, cancellationToken);

            if (project is null)
            {
                return Results.NotFound("Projeto não encontrado.");
            }

            var sourceFile = await sourceFileRepository.GetByIdAsync(
                sourceFileId,
                cancellationToken
            );

            if (sourceFile is null)
            {
                return Results.NotFound("Arquivo não encontrado.");
            }

            if (sourceFile.ProjectId != project.Id)
            {
                return Results.BadRequest("Arquivo não pertence ao projeto informado.");
            }

            const long maxAllowedFileSizeInBytes = 200_000;

            if (sourceFile.Size > maxAllowedFileSizeInBytes)
            {
                return Results.BadRequest("Arquivo muito grande para leitura nesta versão.");
            }

            var fileContent = await gitHubFileContentClient.GetFileContentAsync(
                project.Owner,
                project.RepositoryName,
                sourceFile.GitHubSha,
                cancellationToken
            );

            if (fileContent is null)
            {
                return Results.BadRequest("Não foi possível ler o conteúdo do arquivo no GitHub.");
            }

            var response = new SourceFileContentResponse(
                sourceFile.Id,
                project.Id,
                sourceFile.Path,
                sourceFile.Extension,
                fileContent.Content
            );

            return Results.Ok(response);
        });

        group.MapGet("/{id:guid}/indexing-jobs", async (
            Guid id,
            IProjectRepository projectRepository,
            IIndexingJobRepository indexingJobRepository,
            CancellationToken cancellationToken) =>
        {
            var project = await projectRepository.GetByIdAsync(id, cancellationToken);

            if (project is null)
            {
                return Results.NotFound("Projeto não encontrado.");
            }

            var indexingJobs = await indexingJobRepository.GetByProjectIdAsync(
                project.Id,
                cancellationToken
            );

            var response = indexingJobs
                .Select(indexingJob => new IndexingJobResponse(
                    indexingJob.Id,
                    indexingJob.ProjectId,
                    indexingJob.Status.ToString(),
                    indexingJob.TotalFilesFound,
                    indexingJob.TotalFilesMapped,
                    indexingJob.TotalFilesIgnored,
                    indexingJob.ErrorMessage,
                    indexingJob.CreatedAt,
                    indexingJob.StartedAt,
                    indexingJob.FinishedAt
                ))
                .ToList();

            return Results.Ok(response);
        });

        group.MapGet("/{projectId:guid}/indexing-jobs/{indexingJobId:guid}", async (
            Guid projectId,
            Guid indexingJobId,
            IProjectRepository projectRepository,
            IIndexingJobRepository indexingJobRepository,
            CancellationToken cancellationToken) =>
        {
            var project = await projectRepository.GetByIdAsync(projectId, cancellationToken);

            if (project is null)
            {
                return Results.NotFound("Projeto não encontrado.");
            }

            var indexingJob = await indexingJobRepository.GetByIdAsync(
                indexingJobId,
                cancellationToken
            );

            if (indexingJob is null)
            {
                return Results.NotFound("Job não encontrado.");
            }

            if (indexingJob.ProjectId != project.Id)
            {
                return Results.BadRequest("Job não pertence ao projeto informado.");
            }

            var response = new IndexingJobResponse(
                indexingJob.Id,
                indexingJob.ProjectId,
                indexingJob.Status.ToString(),
                indexingJob.TotalFilesFound,
                indexingJob.TotalFilesMapped,
                indexingJob.TotalFilesIgnored,
                indexingJob.ErrorMessage,
                indexingJob.CreatedAt,
                indexingJob.StartedAt,
                indexingJob.FinishedAt
            );

            return Results.Ok(response);
        });

        group.MapPost("/{projectId:guid}/files/{sourceFileId:guid}/documentation", async (
            Guid projectId,
            Guid sourceFileId,
            IProjectRepository projectRepository,
            ISourceFileRepository sourceFileRepository,
            IFileDocumentationGenerationQueue fileDocumentationGenerationQueue,
            CancellationToken cancellationToken) =>
        {
            var project = await projectRepository.GetByIdAsync(
                projectId,
                cancellationToken
            );

            if (project is null)
            {
                return Results.NotFound("Projeto não encontrado.");
            }

            var sourceFile = await sourceFileRepository.GetByIdAsync(
                sourceFileId,
                cancellationToken
            );

            if (sourceFile is null)
            {
                return Results.NotFound("Arquivo não encontrado.");
            }

            if (sourceFile.ProjectId != project.Id)
            {
                return Results.BadRequest("Arquivo não pertence ao projeto informado.");
            }

            var message = new FileDocumentationGenerationMessage(
                project.Id,
                sourceFile.Id
            );

            await fileDocumentationGenerationQueue.EnqueueAsync(
                message,
                cancellationToken
            );

            var response = new QueueFileDocumentationResponse(
                project.Id,
                sourceFile.Id,
                "queued",
                "Geração de documentação enviada para processamento."
            );

            return Results.Accepted(
                $"/projects/{project.Id}/files/{sourceFile.Id}/documentation",
                response
            );
        });

        group.MapGet("/{projectId:guid}/files/{sourceFileId:guid}/documentation", async (
            Guid projectId,
            Guid sourceFileId,
            IProjectRepository projectRepository,
            ISourceFileRepository sourceFileRepository,
            IFileDocumentationRepository fileDocumentationRepository,
            CancellationToken cancellationToken) =>
        {
            var project = await projectRepository.GetByIdAsync(projectId, cancellationToken);

            if (project is null)
            {
                return Results.NotFound("Projeto não encontrado.");
            }

            var sourceFile = await sourceFileRepository.GetByIdAsync(
                sourceFileId,
                cancellationToken
            );

            if (sourceFile is null)
            {
                return Results.NotFound("Arquivo não encontrado.");
            }

            if (sourceFile.ProjectId != project.Id)
            {
                return Results.BadRequest("Arquivo não pertence ao projeto informado.");
            }

            var documentation = await fileDocumentationRepository.GetBySourceFileIdAsync(
                sourceFile.Id,
                cancellationToken
            );

            if (documentation is null)
            {
                return Results.NotFound("Documentação ainda não foi gerada para este arquivo.");
            }

            var response = new FileDocumentationResponse(
                documentation.Id,
                documentation.ProjectId,
                documentation.SourceFileId,
                documentation.Summary,
                documentation.Content,
                documentation.Generator,
                documentation.CreatedAt
            );

            return Results.Ok(response);
        });

        group.MapPost("/{projectId:guid}/files/documentation", async (
            Guid projectId,
            IProjectRepository projectRepository,
            ISourceFileRepository sourceFileRepository,
            IFileDocumentationGenerationQueue fileDocumentationGenerationQueue,
            CancellationToken cancellationToken) =>
        {
            var project = await projectRepository.GetByIdAsync(
                projectId,
                cancellationToken
            );

            if (project is null)
            {
                return Results.NotFound("Projeto não encontrado.");
            }

            var sourceFiles = await sourceFileRepository.GetByProjectIdAsync(
                project.Id,
                cancellationToken
            );

            const long maxAllowedFileSizeInBytes = 200_000;

            var eligibleFiles = sourceFiles
                .Where(sourceFile => sourceFile.Size <= maxAllowedFileSizeInBytes)
                .Where(sourceFile => IsDocumentableExtension(sourceFile.Extension))
                .ToList();

            foreach (var sourceFile in eligibleFiles)
            {
                var message = new FileDocumentationGenerationMessage(
                    project.Id,
                    sourceFile.Id
                );

                await fileDocumentationGenerationQueue.EnqueueAsync(
                    message,
                    cancellationToken
                );
            }

            var response = new QueueProjectFilesDocumentationResponse(
                project.Id,
                sourceFiles.Count,
                eligibleFiles.Count,
                sourceFiles.Count - eligibleFiles.Count,
                "queued",
                "Geração de documentação enviada para os arquivos elegíveis."
            );

            return Results.Accepted(
                $"/projects/{project.Id}/documentations",
                response
            );
        });

        group.MapGet("/{projectId:guid}/documentations", async (
            Guid projectId,
            IProjectRepository projectRepository,
            IFileDocumentationRepository fileDocumentationRepository,
            CancellationToken cancellationToken) =>
        {
            var project = await projectRepository.GetByIdAsync(
                projectId,
                cancellationToken
            );

            if (project is null)
            {
                return Results.NotFound("Projeto não encontrado.");
            }

            var documentations = await fileDocumentationRepository.GetByProjectIdAsync(
                project.Id,
                cancellationToken
            );

            var response = documentations
                .Select(documentation => new FileDocumentationResponse(
                    documentation.Id,
                    documentation.ProjectId,
                    documentation.SourceFileId,
                    documentation.Summary,
                    documentation.Content,
                    documentation.Generator,
                    documentation.CreatedAt
                ))
                .ToList();

            return Results.Ok(response);
        });

    }

    private static bool IsDocumentableExtension(string extension)
    {
        var documentableExtensions = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase)
    {
        ".cs",
        ".csproj",
        ".sln",
        ".md",
        ".json",
        ".yml",
        ".yaml",
        ".xml",
        ".txt",
        ".js",
        ".ts",
        ".jsx",
        ".tsx",
        ".py",
        ".java",
        ".css",
    };

        return documentableExtensions.Contains(extension);
    }
}
