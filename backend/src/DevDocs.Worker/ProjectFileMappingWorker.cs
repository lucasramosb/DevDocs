using DevDocs.Application.Abstractions;
using DevDocs.Domain.SourceFiles;

namespace DevDocs.Worker;

public sealed class ProjectFileMappingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<ProjectFileMappingWorker> _logger;

    public ProjectFileMappingWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<ProjectFileMappingWorker> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Project file mapping worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var queue = scope.ServiceProvider
                .GetRequiredService<IProjectFileMappingQueue>();

            var message = await queue.DequeueAsync(stoppingToken);

            if (message is null)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                continue;
            }

            try
            {
                await ProcessMessageAsync(
                    message.ProjectId,
                    scope.ServiceProvider,
                    stoppingToken
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Error while mapping files for project {ProjectId}",
                    message.ProjectId
                );
            }
        }
    }

    private async Task ProcessMessageAsync(
        Guid projectId,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var projectRepository = serviceProvider
            .GetRequiredService<IProjectRepository>();

        var sourceFileRepository = serviceProvider
            .GetRequiredService<ISourceFileRepository>();

        var gitHubRepositoryFileClient = serviceProvider
            .GetRequiredService<IGitHubRepositoryFileClient>();

        var unitOfWork = serviceProvider
            .GetRequiredService<IUnitOfWork>();

        var project = await projectRepository.GetByIdAsync(
            projectId,
            cancellationToken
        );

        if (project is null)
        {
            _logger.LogWarning(
                "Project {ProjectId} not found.",
                projectId
            );

            return;
        }

        _logger.LogInformation(
            "Mapping files for project {ProjectId} - {Owner}/{RepositoryName}",
            project.Id,
            project.Owner,
            project.RepositoryName
        );

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
            ".yaml",
            ".ts",
            ".tsx",
            ".js",
            ".jsx",
            ".css",
            ".json",
            ".md"
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

        await sourceFileRepository.DeleteByProjectIdAsync(
            project.Id,
            cancellationToken
        );

        await sourceFileRepository.AddRangeAsync(
            mappedFiles,
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Mapped {MappedFilesCount} files for project {ProjectId}. Total found: {TotalFilesFound}",
            mappedFiles.Count,
            project.Id,
            githubFiles.Count
        );
    }
}
