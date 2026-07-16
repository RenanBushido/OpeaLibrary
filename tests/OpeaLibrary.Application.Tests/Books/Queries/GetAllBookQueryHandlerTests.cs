namespace OpeaLibrary.Application.Tests.Books.Queries;

public class GetAllBookQueryHandlerTests
{
    private static IMapper CreateMapper() =>
        new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance).CreateMapper();

    [Fact]
    public async Task Handle_ReturnsMappedResponseForEachBook()
    {
        var book = Book.Create("Domain-Driven Design", "Eric Evans", 2003);
        var bookRepository = new Mock<IBookRepository>();
        bookRepository.Setup(r => r.GetAllBooksAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([book]);

        var handler = new GetAllBookQueryHandler(bookRepository.Object, CreateMapper());

        var result = await handler.Handle(new GetAllBookRequest(), CancellationToken.None);

        var response = Assert.Single(result);
        Assert.Equal(book.Id, response.Id);
        Assert.Equal(book.Title, response.Title);
        Assert.Equal(book.Author, response.Author);
        Assert.Equal(book.PublishedYear, response.PublishedYear);
        Assert.Equal(book.QuantityAvailable, response.QuantityAvailable);
    }
}
