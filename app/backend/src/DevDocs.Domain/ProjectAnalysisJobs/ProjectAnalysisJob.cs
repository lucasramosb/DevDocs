using System;

namespace DevDocs.Domain.ProjectAnalysisJobs;

public class ProjectAnalysisJob
{
    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public ProjectAnalysisStatus Status { get; private set; }
    public ProjectAnalysisStep CurrentStep { get; private set; }
    public int Progress { get; private set; }
    public int FilesFound { get; private set; }
    public int FilesProcessed { get; private set; }
    public int FilesDocumented { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? FinishedAt { get; private set; }

    private ProjectAnalysisJob()
    {
    }

    public ProjectAnalysisJob(Guid projectId)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Status = ProjectAnalysisStatus.Pending;
        CurrentStep = ProjectAnalysisStep.Pending;
        Progress = 0;
        FilesFound = 0;
        FilesProcessed = 0;
        FilesDocumented = 0;
        CreatedAt = DateTime.UtcNow;
    }

    public void Start(ProjectAnalysisStep step)
    {
        Status = ProjectAnalysisStatus.Running;
        CurrentStep = step;
        StartedAt ??= DateTime.UtcNow;
    }

    public void UpdateStep(ProjectAnalysisStep step, int progress)
    {
        CurrentStep = step;
        Progress = progress;
    }

    public void UpdateFilesFound(int filesFound)
    {
        FilesFound = filesFound;
    }

    public void IncrementFilesProcessed()
    {
        FilesProcessed++;
    }

    public void IncrementFilesDocumented()
    {
        FilesDocumented++;
    }

    public void SetProgress(int progress)
    {
        Progress = progress;
    }

    public void Complete()
    {
        Status = ProjectAnalysisStatus.Completed;
        CurrentStep = ProjectAnalysisStep.Completed;
        Progress = 100;
        FinishedAt = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        Status = ProjectAnalysisStatus.Failed;
        ErrorMessage = errorMessage;
        FinishedAt = DateTime.UtcNow;
    }
}
