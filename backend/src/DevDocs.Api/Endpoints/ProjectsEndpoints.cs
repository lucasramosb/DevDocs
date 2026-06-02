using DevDocs.Application.Abstractions;
using DevDocs.Application.GitHub;
using DevDocs.Application.Projects.DTOs;
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
    }
}