using DevDocs.Application.Abstractions;
using DevDocs.Infrastructure.Documentation;
using DevDocs.Infrastructure.GitHub;
using DevDocs.Infrastructure.Persistence;
using DevDocs.Infrastructure.Persistence.Repositories;
using DevDocs.Infrastructure.Queue;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace DevDocs.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        services.AddDbContext<DevDocsDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISourceFileRepository, SourceFileRepository>();
        services.AddScoped<IIndexingJobRepository, IndexingJobRepository>();
        services.AddScoped<IFileDocumentationRepository, FileDocumentationRepository>();
        services.AddScoped<IFileDocumentationGenerator, SimpleFileDocumentationGenerator>();
        services.AddScoped<IProjectFileMappingQueue, RedisProjectFileMappingQueue>();
        services.AddScoped<IProjectDocumentationRepository, ProjectDocumentationRepository>();
        services.AddScoped<IProjectDocumentationGenerator, SimpleProjectDocumentationGenerator>();

        var redisConnectionString = configuration["Redis:ConnectionString"]
            ?? "localhost:6379";

        services.AddSingleton(_ =>
            new Lazy<Task<IConnectionMultiplexer>>(async () =>
            {
                var options = ConfigurationOptions.Parse(redisConnectionString);

                options.AbortOnConnectFail = false;
                options.ConnectRetry = 1;
                options.ConnectTimeout = 1000;

                return await ConnectionMultiplexer.ConnectAsync(options);
            }));

        services.AddHttpClient<IGitHubRepositoryClient, GitHubRepositoryClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.github.com");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("DevDocs");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        });

        services.AddHttpClient<IGitHubRepositoryFileClient, GitHubRepositoryFileClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.github.com");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("DevDocs");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        });

        services.AddHttpClient<IGitHubFileContentClient, GitHubFileContentClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.github.com");
            client.DefaultRequestHeaders.UserAgent.ParseAdd("DevDocs");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        });

        services.AddScoped<
            IFileDocumentationGenerationQueue,
            RedisFileDocumentationGenerationQueue>();

        return services;
    }
}
