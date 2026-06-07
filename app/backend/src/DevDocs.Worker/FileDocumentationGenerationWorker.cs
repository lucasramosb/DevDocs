using DevDocs.Application.Abstractions;
using DevDocs.Domain.FileDocumentations;

namespace DevDocs.Worker;

public sealed class FileDocumentationGenerationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<FileDocumentationGenerationWorker> _logger;

    public FileDocumentationGenerationWorker(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<FileDocumentationGenerationWorker> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("File documentation generation worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _serviceScopeFactory.CreateScope();

            var queue = scope.ServiceProvider
                .GetRequiredService<IFileDocumentationGenerationQueue>();

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
                    message.SourceFileId,
                    scope.ServiceProvider,
                    stoppingToken
                );
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Error while generating documentation for project {ProjectId} and file {SourceFileId}",
                    message.ProjectId,
                    message.SourceFileId
                );
            }
        }
    }

    private async Task ProcessMessageAsync(
        Guid projectId,
        Guid sourceFileId,
        IServiceProvider serviceProvider,
        CancellationToken cancellationToken)
    {
        var projectRepository = serviceProvider
            .GetRequiredService<IProjectRepository>();

        var sourceFileRepository = serviceProvider
            .GetRequiredService<ISourceFileRepository>();

        var fileDocumentationRepository = serviceProvider
            .GetRequiredService<IFileDocumentationRepository>();

        var fileDocumentationGenerator = serviceProvider
            .GetRequiredService<IFileDocumentationGenerator>();

        var gitHubFileContentClient = serviceProvider
            .GetRequiredService<IGitHubFileContentClient>();

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

        var sourceFile = await sourceFileRepository.GetByIdAsync(
            sourceFileId,
            cancellationToken
        );

        if (sourceFile is null)
        {
            _logger.LogWarning(
                "Source file {SourceFileId} not found.",
                sourceFileId
            );

            return;
        }

        if (sourceFile.ProjectId != project.Id)
        {
            _logger.LogWarning(
                "Source file {SourceFileId} does not belong to project {ProjectId}.",
                sourceFile.Id,
                project.Id
            );

            return;
        }

        const long maxAllowedFileSizeInBytes = 200_000;

        if (sourceFile.Size > maxAllowedFileSizeInBytes)
        {
            _logger.LogWarning(
                "Source file {SourceFileId} is too large. Size: {Size}",
                sourceFile.Id,
                sourceFile.Size
            );

            return;
        }

        _logger.LogInformation(
            "Generating documentation for file {FilePath}",
            sourceFile.Path
        );

        var fileContent = await gitHubFileContentClient.GetFileContentAsync(
            project.Owner,
            project.RepositoryName,
            sourceFile.GitHubSha,
            cancellationToken
        );

        if (fileContent is null)
        {
            _logger.LogWarning(
                "Could not read content for source file {SourceFileId}.",
                sourceFile.Id
            );

            return;
        }

        var generatedDocumentation = fileDocumentationGenerator.Generate(
            sourceFile.Path,
            sourceFile.Extension,
            fileContent.Content
        );

        await fileDocumentationRepository.DeleteBySourceFileIdAsync(
            sourceFile.Id,
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var documentation = new FileDocumentation(
            sourceFile.Id,
            project.Id,
            generatedDocumentation.Summary,
            generatedDocumentation.Content,
            generatedDocumentation.Generator
        );

        await fileDocumentationRepository.AddAsync(
            documentation,
            cancellationToken
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Documentation generated for file {SourceFileId} - {FilePath}",
            sourceFile.Id,
            sourceFile.Path
        );
    }
}
