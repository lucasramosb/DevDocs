using DevDocs.Application.Abstractions;
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
            IProjectRepository projectRepository,
            IUnitOfWork unitOfWork,
            CancellationToken cancellationToken) =>
        {
            var project = new Project(
                request.Name,
                request.RepositoryPath,
                request.Description
            );

            await projectRepository.AddAsync(project, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new ProjectResponse(
                project.Id,
                project.Name,
                project.RepositoryPath,
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
                project.RepositoryPath,
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

            if (project == null)
            {
                return Results.NotFound();
            }

            var response = new ProjectResponse(
                project.Id,
                project.Name,
                project.RepositoryPath,
                project.Description,
                project.CreatedAt
            );

            return Results.Ok(response);
        });
    }
}