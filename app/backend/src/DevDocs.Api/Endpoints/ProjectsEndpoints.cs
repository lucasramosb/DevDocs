using DevDocs.Application.Abstractions;
using DevDocs.Application.GitHub;
using DevDocs.Application.ProjectAnalysisJobs.DTOs;
using DevDocs.Application.Projects.DTOs;
using DevDocs.Application.Queue;
using DevDocs.Domain.ProjectAnalysisJobs;
using DevDocs.Domain.Projects;
using DevDocs.Application.ProjectDocumentations.DTOs;

namespace DevDocs.Api.Endpoints;

public static class ProjectsEndpoints
{
    public static void MapProjectsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/projects")
            .WithTags("Projects");

        group.MapPost("/analyze", async (
            AnalyzeProjectRequest request,
            IGitHubRepositoryClient gitHubRepositoryClient,
            IProjectRepository projectRepository,
            IProjectAnalysisJobRepository jobRepository,
            IProjectAnalysisQueue queue,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            if (!GitHubRepositoryUrl.TryParse(request.GithubUrl, out var repositoryUrl) ||
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

            var project = await projectRepository.GetByGitHubUrlAsync(
                repositoryInfo.HtmlUrl,
                cancellationToken
            );

            if (project is null)
            {
                project = new Project(
                    repositoryInfo.Name,
                    repositoryInfo.Owner,
                    repositoryInfo.Name,
                    repositoryInfo.HtmlUrl,
                    repositoryInfo.DefaultBranch,
                    repositoryInfo.Description
                );

                await projectRepository.AddAsync(project, cancellationToken);
            }
            
            var job = new ProjectAnalysisJob(project.Id);
            await jobRepository.AddAsync(job, cancellationToken);
            
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var message = new ProjectAnalysisMessage(project.Id, job.Id);
            await queue.EnqueueAsync(message, cancellationToken);

            var response = new AnalyzeProjectResponse(
                project.Id,
                job.Id,
                job.Status.ToString()
            );

            return Results.Accepted($"/projects/{project.Id}/analysis-jobs/{job.Id}", response);
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

        group.MapGet("/{projectId:guid}/analysis-jobs/{jobId:guid}", async (
            Guid projectId,
            Guid jobId,
            IProjectRepository projectRepository,
            IProjectAnalysisJobRepository jobRepository,
            CancellationToken cancellationToken) =>
        {
            var project = await projectRepository.GetByIdAsync(projectId, cancellationToken);

            if (project is null)
            {
                return Results.NotFound("Projeto não encontrado.");
            }

            var job = await jobRepository.GetByIdAsync(jobId, cancellationToken);

            if (job is null)
            {
                return Results.NotFound("Job não encontrado.");
            }

            if (job.ProjectId != project.Id)
            {
                return Results.BadRequest("Job não pertence ao projeto informado.");
            }

            var response = new ProjectAnalysisJobResponse(
                job.Id,
                job.ProjectId,
                job.Status.ToString(),
                job.CurrentStep.ToString(),
                job.Progress,
                job.FilesFound,
                job.FilesProcessed,
                job.FilesDocumented,
                job.ErrorMessage,
                job.CreatedAt,
                job.StartedAt,
                job.FinishedAt
            );

            return Results.Ok(response);
        });

        group.MapGet("/{projectId:guid}/documentation", async (
            Guid projectId,
            IProjectRepository projectRepository,
            IProjectDocumentationRepository projectDocumentationRepository,
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

            var documentation = await projectDocumentationRepository.GetByProjectIdAsync(
                project.Id,
                cancellationToken
            );

            if (documentation is null)
            {
                return Results.NotFound(
                    "Documentação geral ainda não foi gerada para este projeto."
                );
            }

            var response = new ProjectDocumentationResponse(
                documentation.Id,
                documentation.ProjectId,
                documentation.Title,
                documentation.Overview,
                documentation.Architecture,
                documentation.MainFlows,
                documentation.Technologies,
                documentation.Content,
                documentation.Generator,
                documentation.CreatedAt
            );

            return Results.Ok(response);
        });
    }
}
