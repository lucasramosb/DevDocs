namespace DevDocs.Domain.SourceFiles;

public class SourceFile
{
    public Guid Id { get; private set; }

    public Guid ProjectId { get; private set; }

    public string Path { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public string Extension { get; private set; } = string.Empty;

    public string GitHubSha { get; private set; } = string.Empty;

    public string GitHubBlobUrl { get; private set; } = string.Empty;

    public long Size { get; private set; }

    public bool IsDocumentationFile { get; private set; }

    public bool IsTestFile { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private SourceFile()
    {
    }

    public SourceFile(
        Guid projectId,
        string path,
        string name,
        string extension,
        string gitHubSha,
        string gitHubBlobUrl,
        long size,
        bool isDocumentationFile,
        bool isTestFile)
    {
        Id = Guid.NewGuid();
        ProjectId = projectId;
        Path = path;
        Name = name;
        Extension = extension;
        GitHubSha = gitHubSha;
        GitHubBlobUrl = gitHubBlobUrl;
        Size = size;
        IsDocumentationFile = isDocumentationFile;
        IsTestFile = isTestFile;
        CreatedAt = DateTime.UtcNow;
    }
}
