namespace DevDocs.Domain.IndexingJobs;

public class IndexingJob
{
    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public IndexingJobStatus Status { get; private set; }

    public int TotalFilesFound { get; private set; }

    public int TotalFilesMapped { get; private set; }

    public int TotalFilesIgnored { get; private set; }

    public string? ErrorMessage { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? FinishedAt { get; private set; }

    private IndexingJob()
    {
    }

    public IndexingJob(Guid projectId)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Status = IndexingJobStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsRunning()
    {
        Status = IndexingJobStatus.Running;
        StartedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    public void MarkAsCompleted(
        int totalFilesFound,
        int totalFilesMapped,
        int totalFilesIgnored)
    {
        Status = IndexingJobStatus.Completed;
        TotalFilesFound = totalFilesFound;
        TotalFilesMapped = totalFilesMapped;
        TotalFilesIgnored = totalFilesIgnored;
        FinishedAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    public void MarkAsFailed(string errorMessage)
    {
        Status = IndexingJobStatus.Failed;
        ErrorMessage = errorMessage;
        FinishedAt = DateTime.UtcNow;
    }
}
