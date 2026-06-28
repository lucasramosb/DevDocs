using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DevDocs.Application.Queue;
using StackExchange.Redis;

namespace DevDocs.Infrastructure.Queue;

public sealed class RedisProjectAnalysisQueue : IProjectAnalysisQueue
{
    private const string QueueKey = "devdocs:project-analysis-jobs";

    private readonly Lazy<Task<IConnectionMultiplexer>> _connectionMultiplexer;

    public RedisProjectAnalysisQueue(
        Lazy<Task<IConnectionMultiplexer>> connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task EnqueueAsync(
        ProjectAnalysisMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var connection = await _connectionMultiplexer.Value.WaitAsync(cancellationToken);
        var database = connection.GetDatabase();

        var payload = JsonSerializer.Serialize(message);

        await database.ListLeftPushAsync(QueueKey, payload);
    }

    public async Task<ProjectAnalysisMessage?> DequeueAsync(
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

        return JsonSerializer.Deserialize<ProjectAnalysisMessage>(
            payload.ToString()
        );
    }
}
