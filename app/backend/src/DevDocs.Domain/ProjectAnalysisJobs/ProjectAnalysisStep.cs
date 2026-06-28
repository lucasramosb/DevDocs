namespace DevDocs.Domain.ProjectAnalysisJobs;

public enum ProjectAnalysisStep
{
    Pending,
    DownloadingRepository,
    MappingFiles,
    GeneratingFileDocumentation,
    GeneratingProjectDocumentation,
    Completed
}
