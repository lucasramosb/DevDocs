using DevDocs.Domain.FileDocumentations;
using DevDocs.Domain.IndexingJobs;
using DevDocs.Domain.Projects;
using DevDocs.Domain.SourceFiles;
using Microsoft.EntityFrameworkCore;

namespace DevDocs.Infrastructure.Persistence;

public class DevDocsDbContext : DbContext
{
    public DevDocsDbContext(DbContextOptions<DevDocsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<SourceFile> SourceFiles => Set<SourceFile>();

    public DbSet<IndexingJob> IndexingJobs => Set<IndexingJob>();

    public DbSet<FileDocumentation> FileDocumentations => Set<FileDocumentation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Project>(builder =>
        {
            builder.ToTable("Projects");

            builder.HasKey(project => project.Id);

            builder.Property(project => project.Name)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(project => project.Owner)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(project => project.RepositoryName)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(project => project.GitHubUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(project => project.DefaultBranch)
                .IsRequired()
                .HasMaxLength(120);

            builder.Property(project => project.Description)
                .HasMaxLength(1000);

            builder.Property(project => project.CreatedAt)
                .IsRequired();

            builder.HasIndex(project => project.GitHubUrl)
                .IsUnique();
        });

        modelBuilder.Entity<SourceFile>(builder =>
        {
            builder.ToTable("SourceFiles");

            builder.HasKey(sourceFile => sourceFile.Id);

            builder.Property(sourceFile => sourceFile.ProjectId)
                .IsRequired();

            builder.Property(sourceFile => sourceFile.Path)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(sourceFile => sourceFile.Name)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(sourceFile => sourceFile.Extension)
                .IsRequired()
                .HasMaxLength(30);

            builder.Property(sourceFile => sourceFile.GitHubSha)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(sourceFile => sourceFile.GitHubBlobUrl)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(sourceFile => sourceFile.Size)
                .IsRequired();

            builder.Property(sourceFile => sourceFile.IsDocumentationFile)
                .IsRequired();

            builder.Property(sourceFile => sourceFile.IsTestFile)
                .IsRequired();

            builder.Property(sourceFile => sourceFile.CreatedAt)
                .IsRequired();

            builder.HasIndex(sourceFile => new
            {
                sourceFile.ProjectId,
                sourceFile.Path
            }).IsUnique();
        });

        modelBuilder.Entity<IndexingJob>(builder =>
        {
            builder.ToTable("IndexingJobs");

            builder.HasKey(indexingJob => indexingJob.Id);

            builder.Property(indexingJob => indexingJob.ProjectId)
                .IsRequired();

            builder.Property(indexingJob => indexingJob.Status)
                .HasConversion<string>()
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(indexingJob => indexingJob.TotalFilesFound)
                .IsRequired();

            builder.Property(indexingJob => indexingJob.TotalFilesMapped)
                .IsRequired();

            builder.Property(indexingJob => indexingJob.TotalFilesIgnored)
                .IsRequired();

            builder.Property(indexingJob => indexingJob.ErrorMessage)
                .HasMaxLength(2000);

            builder.Property(indexingJob => indexingJob.CreatedAt)
                .IsRequired();

            builder.Property(indexingJob => indexingJob.StartedAt);

            builder.Property(indexingJob => indexingJob.FinishedAt);

            builder.HasIndex(indexingJob => indexingJob.ProjectId);
        });

        modelBuilder.Entity<FileDocumentation>(builder =>
        {
            builder.ToTable("FileDocumentations");

            builder.HasKey(documentation => documentation.Id);

            builder.Property(documentation => documentation.ProjectId)
                .IsRequired();

            builder.Property(documentation => documentation.SourceFileId)
                .IsRequired();

            builder.Property(documentation => documentation.Summary)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(documentation => documentation.Content)
                .IsRequired();

            builder.Property(documentation => documentation.Generator)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(documentation => documentation.CreatedAt)
                .IsRequired();

            builder.HasIndex(documentation => documentation.SourceFileId)
                .IsUnique();

            builder.HasIndex(documentation => documentation.ProjectId);
        });
    }
}
