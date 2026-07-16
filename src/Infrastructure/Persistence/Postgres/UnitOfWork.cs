namespace OpeaLibrary.Infrastructure.Persistence.Postgres;

public sealed class UnitOfWork(OpeaLibraryDbContext dbContext) : IUnitOfWork
{
    public IBookWriteRepository BookWriteRepository { get; } = new BookWriteRepository(dbContext);
    public ILoanWriteRepository LoanWriteRepository { get; } = new LoanWriteRepository(dbContext);

    public async Task CommitAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
