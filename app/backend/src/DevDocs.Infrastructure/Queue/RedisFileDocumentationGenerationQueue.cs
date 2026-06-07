using System.Text.Json;
using DevDocs.Application.Abstractions;
using DevDocs.Application.Queue;
using StackExchange.Redis;

namespace DevDocs.Infrastructure.Queue;

public sealed class RedisFileDocumentationGenerationQueue
    : IFileDocumentationGenerationQueue
{
    private const string QueueKey = "devdocs:file-documentation-generation-jobs";

    private readonly Lazy<Task<IConnectionMultiplexer>> _connectionMultiplexer;

    public RedisFileDocumentationGenerationQueue(
        Lazy<Task<IConnectionMultiplexer>> connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task EnqueueAsync(
        FileDocumentationGenerationMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var connection = await _connectionMultiplexer.Value.WaitAsync(cancellationToken);
        var database = connection.GetDatabase();

        var payload = JsonSerializer.Serialize(message);

        await database.ListLeftPushAsync(QueueKey, payload);
    }

    public async Task<FileDocumentationGenerationMessage?> DequeueAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var connection = await _connectionMultiplexer.Value.WaitAsync(cancellationToken);
        var database = connection.GetDatabase();

        var payload = await database.ListRightPopAsync(QueueKey);

        if (!payload.HasValue)
        {
            return null;
        }

        return JsonSerializer.Deserialize<FileDocumentationGenerationMessage>(
            payload.ToString()
        );
    }
}
