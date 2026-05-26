using DevDocs.Application.Abstractions;

namespace DevDocs.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly DevDocsDbContext _dbContext;

    public UnitOfWork(DevDocsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}