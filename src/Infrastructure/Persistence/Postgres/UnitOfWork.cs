namespace OpeaLibrary.Infrastructure.Persistence.Postgres;

public sealed class UnitOfWork(OpeaLibraryDbContext dbContext) : IUnitOfWork
{
    public IBookRepository BookRepository { get; } = new BookRepository(dbContext);
    public ILoanRepository LoanRepository { get; } = new LoanRepository(dbContext);

    public async Task CommitAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);
}
