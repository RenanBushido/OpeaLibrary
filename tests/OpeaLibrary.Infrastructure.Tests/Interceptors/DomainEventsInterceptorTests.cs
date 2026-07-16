namespace OpeaLibrary.Infrastructure.Tests.Interceptors;

public class DomainEventsInterceptorTests
{
    [Fact]
    public async Task SavedChangesAsync_OnSuccessfulSave_DispatchesAndClearsDomainEvents()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var interceptor = new DomainEventsInterceptor(mediator.Object);
        using var database = TestDbContextFactory.Create(interceptor);
        var dbContext = database.Context;
        var book = Book.Create("Clean Code", "Robert C. Martin", 2008, 3);

        dbContext.Books.Add(book);
        await dbContext.SaveChangesAsync();

        mediator.Verify(
            m => m.Publish(
                It.Is<INotification>(n => ((DomainEventNotification<BookCreatedEvent>)n).DomainEvent.BookId == book.Id),
                It.IsAny<CancellationToken>()),
            Times.Once);
        Assert.Empty(book.DomainEvents);
    }

    [Fact]
    public async Task SavedChangesAsync_OnFailedSave_DispatchesNothing()
    {
        var mediator = new Mock<IMediator>();
        mediator
            .Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var interceptor = new DomainEventsInterceptor(mediator.Object);
        using var database = TestDbContextFactory.Create(interceptor);
        var dbContext = database.Context;
        var loan = Loan.Create(Guid.NewGuid());

        dbContext.Loans.Add(loan);

        await Assert.ThrowsAsync<DbUpdateException>(() => dbContext.SaveChangesAsync());

        mediator.Verify(
            m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
