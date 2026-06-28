using System.Threading;
using System.Threading.Tasks;

namespace DevDocs.Application.Queue;

public interface IProjectAnalysisQueue
{
    Task EnqueueAsync(ProjectAnalysisMessage message, CancellationToken cancellationToken = default);
    Task<ProjectAnalysisMessage?> DequeueAsync(CancellationToken cancellationToken = default);
}
