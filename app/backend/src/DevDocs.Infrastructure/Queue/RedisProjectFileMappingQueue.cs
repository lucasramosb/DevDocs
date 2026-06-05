using System.Text.Json;
using DevDocs.Application.Abstractions;
using DevDocs.Application.Queue;
using StackExchange.Redis;

namespace DevDocs.Infrastructure.Queue;

public sealed class RedisProjectFileMappingQueue : IProjectFileMappingQueue
{
    //nome da fila
    private const string QueueKey = "devdocs:project-file-mapping-jobs";

    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public RedisProjectFileMappingQueue(IConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task EnqueueAsync(
        ProjectFileMappingMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var database = _connectionMultiplexer.GetDatabase();

        var payload = JsonSerializer.Serialize(message);

        await database.ListLeftPushAsync(QueueKey, payload);
    }

    public async Task<ProjectFileMappingMessage?> DequeueAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var database = _connectionMultiplexer.GetDatabase();

        var payload = await database.ListRightPopAsync(QueueKey);

        if (!payload.HasValue)
        {
            return null;
        }

        return JsonSerializer.Deserialize<ProjectFileMappingMessage>(
            payload.ToString()
        );
    }
}
