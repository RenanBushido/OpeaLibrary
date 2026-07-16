namespace OpeaLibrary.Application.Tests.Books.Queries;

public class GetBookByIdQueryHandlerTests
{
    private static IMapper CreateMapper() =>
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance).CreateMapper();

    [Fact]
    public async Task Handle_ReturnsMappedResponse()
    {
        var book = Book.Create("Refactoring", "Martin Fowler", 1999);
        var bookRepository = new Mock<IBookRepository>();
        bookRepository.Setup(r => r.GetBookByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var handler = new GetBookByIdQueryHandler(bookRepository.Object, CreateMapper());

        var response = await handler.Handle(new GetBookByIdRequest(book.Id), CancellationToken.None);

        Assert.Equal(book.Id, response.Id);
        Assert.Equal(book.Title, response.Title);
        Assert.Equal(book.Author, response.Author);
        Assert.Equal(book.PublishedYear, response.PublishedYear);
        Assert.Equal(book.QuantityAvailable, response.QuantityAvailable);
    }

    [Fact]
    public async Task Handle_WhenBookDoesNotExist_ThrowsKeyNotFoundException()
    {
        var bookId = Guid.NewGuid();
        var bookRepository = new Mock<IBookRepository>();
        bookRepository.Setup(r => r.GetBookByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var handler = new GetBookByIdQueryHandler(bookRepository.Object, CreateMapper());

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => handler.Handle(new GetBookByIdRequest(bookId), CancellationToken.None));
    }
}
