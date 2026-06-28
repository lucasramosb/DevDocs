using System.Diagnostics;
using DevDocs.Application.Abstractions;
using DevDocs.Application.Queue;
using DevDocs.Domain.FileDocumentations;
using DevDocs.Domain.ProjectAnalysisJobs;
using DevDocs.Domain.ProjectDocumentations;
using DevDocs.Domain.SourceFiles;
using DevDocs.Application.ProjectDocumentations.DTOs;
using DevDocs.Application.FileDocumentations.DTOs;

namespace DevDocs.Worker;

public class ProjectAnalysisWorker : BackgroundService
{
    private readonly ILogger<ProjectAnalysisWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IProjectAnalysisQueue _queue;

    public ProjectAnalysisWorker(
        ILogger<ProjectAnalysisWorker> logger,
        IServiceScopeFactory scopeFactory,
        IProjectAnalysisQueue queue)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _queue = queue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ProjectAnalysisWorker is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var message = await _queue.DequeueAsync(stoppingToken);

                if (message is null)
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                _logger.LogInformation(
                    "Received ProjectAnalysisMessage for Job {JobId} and Project {ProjectId}",
                    message.JobId,
                    message.ProjectId
                );

                await ProcessJobAsync(message, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                // Worker is stopping
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the queue.");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }

    private async Task ProcessJobAsync(ProjectAnalysisMessage message, CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var jobRepository = scope.ServiceProvider.GetRequiredService<IProjectAnalysisJobRepository>();
        var projectRepository = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        var sourceFileRepository = scope.ServiceProvider.GetRequiredService<ISourceFileRepository>();
        var fileDocRepository = scope.ServiceProvider.GetRequiredService<IFileDocumentationRepository>();
        var projDocRepository = scope.ServiceProvider.GetRequiredService<IProjectDocumentationRepository>();
        
        var gitHubFileClient = scope.ServiceProvider.GetRequiredService<IGitHubRepositoryFileClient>();
        var fileDocGenerator = scope.ServiceProvider.GetRequiredService<IFileDocumentationGenerator>();
        var projDocGenerator = scope.ServiceProvider.GetRequiredService<IProjectDocumentationGenerator>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var job = await jobRepository.GetByIdAsync(message.JobId, stoppingToken);
        if (job is null)
        {
            _logger.LogWarning("Job {JobId} not found.", message.JobId);
            return;
        }

        var project = await projectRepository.GetByIdAsync(message.ProjectId, stoppingToken);
        if (project is null)
        {
            _logger.LogWarning("Project {ProjectId} not found.", message.ProjectId);
            job.Fail("Projeto não encontrado.");
            await jobRepository.UpdateAsync(job, stoppingToken);
            await unitOfWork.SaveChangesAsync(stoppingToken);
            return;
        }

        try
        {
            // 1. Start / Downloading Repository (mock logic for downloading, we just map files usually)
            job.Start(ProjectAnalysisStep.DownloadingRepository);
            await jobRepository.UpdateAsync(job, stoppingToken);
            await unitOfWork.SaveChangesAsync(stoppingToken);

            // 2. Mapping Files
            job.UpdateStep(ProjectAnalysisStep.MappingFiles, 10);
            await jobRepository.UpdateAsync(job, stoppingToken);
            await unitOfWork.SaveChangesAsync(stoppingToken);

            var treeFiles = await gitHubFileClient.GetRepositoryFilesAsync(
                project.Owner,
                project.RepositoryName,
                project.DefaultBranch,
                stoppingToken
            );

            // Save files to DB (Filtering happens inside the logic or before save)
            var filesToSave = treeFiles
                .Where(f => !f.Path.Contains("node_modules") && !f.Path.StartsWith(".git"))
                .Select(f => new SourceFile(
                    project.Id,
                    f.Path,
                    f.Path.Split('/').Last(),
                    Path.GetExtension(f.Path),
                    f.Sha,
                    $"https://github.com/{project.Owner}/{project.RepositoryName}/blob/{project.DefaultBranch}/{f.Path}",
                    f.Size,
                    false, // is doc
                    false  // is test
                )).ToList();

            // Clear old files for this project
            await sourceFileRepository.DeleteByProjectIdAsync(project.Id, stoppingToken);
            await sourceFileRepository.AddRangeAsync(filesToSave, stoppingToken);
            await unitOfWork.SaveChangesAsync(stoppingToken);

            job.UpdateFilesFound(filesToSave.Count);
            
            // 3. Generating File Documentation
            job.UpdateStep(ProjectAnalysisStep.GeneratingFileDocumentation, 20);
            await jobRepository.UpdateAsync(job, stoppingToken);
            await unitOfWork.SaveChangesAsync(stoppingToken);

            var gitHubFileContentClient = scope.ServiceProvider.GetRequiredService<IGitHubFileContentClient>();

            foreach (var file in filesToSave)
            {
                if (file.Size <= 200000 && IsDocumentableExtension(file.Extension))
                {
                    var contentResponse = await gitHubFileContentClient.GetFileContentAsync(
                        project.Owner,
                        project.RepositoryName,
                        file.GitHubSha,
                        stoppingToken
                    );

                    if (contentResponse is not null)
                    {
                        var genResult = await fileDocGenerator.GenerateAsync(
                            file.Path,
                            file.Extension,
                            contentResponse.Content,
                            stoppingToken
                        );

                        var fileDoc = new FileDocumentation(
                            file.Id,
                            project.Id,
                            genResult.Summary,
                            genResult.Content,
                            genResult.Generator
                        );

                        await fileDocRepository.AddAsync(fileDoc, stoppingToken);
                        await unitOfWork.SaveChangesAsync(stoppingToken);
                        
                        job.IncrementFilesDocumented();
                    }
                }

                job.IncrementFilesProcessed();
                
                // Update progress on every file for better UI feedback
                await jobRepository.UpdateAsync(job, stoppingToken);
                await unitOfWork.SaveChangesAsync(stoppingToken);
            }

            // 4. Generating Project Documentation
            job.UpdateStep(ProjectAnalysisStep.GeneratingProjectDocumentation, 80);
            await jobRepository.UpdateAsync(job, stoppingToken);
            await unitOfWork.SaveChangesAsync(stoppingToken);

            var allFileDocs = await fileDocRepository.GetByProjectIdAsync(project.Id, stoppingToken);
            var sourceFilesById = filesToSave.ToDictionary(f => f.Id, f => f);

            var fileContexts = allFileDocs
                .Where(doc => sourceFilesById.ContainsKey(doc.SourceFileId))
                .Select(doc =>
                {
                    var sf = sourceFilesById[doc.SourceFileId];
                    return new ProjectFileDocumentationContext(sf.Id, sf.Path, sf.Extension, doc.Summary, doc.Generator);
                }).ToList();

            var projectContext = new ProjectDocumentationContext(
                project.Name,
                project.Owner,
                project.RepositoryName,
                project.GitHubUrl,
                project.DefaultBranch,
                fileContexts
            );

            var projectGenResult = projDocGenerator.Generate(projectContext);
            var projDoc = new ProjectDocumentation(
                project.Id,
                projectGenResult.Title,
                projectGenResult.Overview,
                projectGenResult.Architecture,
                projectGenResult.MainFlows,
                projectGenResult.Technologies,
                projectGenResult.Content,
                projectGenResult.Generator
            );

            await projDocRepository.DeleteByProjectIdAsync(project.Id, stoppingToken);
            await projDocRepository.AddAsync(projDoc, stoppingToken);
            await unitOfWork.SaveChangesAsync(stoppingToken);

            // 5. Completed
            job.Complete();
            await jobRepository.UpdateAsync(job, stoppingToken);
            await unitOfWork.SaveChangesAsync(stoppingToken);

            _logger.LogInformation("Job {JobId} completed successfully.", job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Job {JobId}", job.Id);
            job.Fail(ex.Message);
            await jobRepository.UpdateAsync(job, stoppingToken);
            await unitOfWork.SaveChangesAsync(stoppingToken);
        }
    }

    private static bool IsDocumentableExtension(string extension)
    {
        var documentableExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".cs", ".csproj", ".sln", ".js", ".jsx", ".ts", ".tsx",
            ".css", ".scss", ".md", ".json", ".yml", ".yaml"
        };
        return documentableExtensions.Contains(extension);
    }
}
