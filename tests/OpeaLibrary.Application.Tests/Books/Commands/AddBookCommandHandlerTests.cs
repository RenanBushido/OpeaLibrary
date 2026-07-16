namespace OpeaLibrary.Application.Tests.Books.Commands;

public class AddBookCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_CreatesBookAndCommits()
    {
        var bookRepository = new Mock<IBookRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.BookRepository).Returns(bookRepository.Object);

        var handler = new AddBookCommandHandler(unitOfWork.Object);
        var command = new AddBookCommand("Clean Code", "Robert C. Martin", 2008, 3);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.NotEqual(Guid.Empty, result);
        bookRepository.Verify(
            r => r.AddBookAsync(It.Is<Book>(b => b.Id == result && b.Title == command.Title && b.Author == command.Author && b.PublishedYear == command.PublishedYear && b.QuantityAvailable == command.QuantityAvailable)),
            Times.Once);
        unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
