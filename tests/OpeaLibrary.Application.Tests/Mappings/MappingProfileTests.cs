namespace OpeaLibrary.Application.Tests.Mappings;

public class MappingProfileTests
{
    private static MapperConfiguration CreateConfiguration() =>
        new(cfg => cfg.AddProfile<MappingProfile>(), NullLoggerFactory.Instance);

    [Fact]
    public void Configuration_IsValid()
    {
        var configuration = CreateConfiguration();

        configuration.AssertConfigurationIsValid();
    }

    [Fact]
    public void Map_BookToGetBookByIdResponse_MapsAllFields()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var book = Book.Create("1984", "George Orwell", 1949);

        var response = mapper.Map<GetBookByIdResponse>(book);

        Assert.Equal(book.Id, response.Id);
        Assert.Equal(book.Title, response.Title);
        Assert.Equal(book.Author, response.Author);
        Assert.Equal(book.PublishedYear, response.PublishedYear);
        Assert.Equal(book.QuantityAvailable, response.QuantityAvailable);
    }

    [Fact]
    public void Map_BookToGetAllBookResponse_MapsAllFields()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var book = Book.Create("Brave New World", "Aldous Huxley", 1932);

        var response = mapper.Map<GetAllBookResponse>(book);

        Assert.Equal(book.Id, response.Id);
        Assert.Equal(book.Title, response.Title);
        Assert.Equal(book.Author, response.Author);
        Assert.Equal(book.PublishedYear, response.PublishedYear);
        Assert.Equal(book.QuantityAvailable, response.QuantityAvailable);
    }

    [Fact]
    public void Map_LoanToGetAllLoansResponse_MapsAllFields()
    {
        var mapper = CreateConfiguration().CreateMapper();
        var loan = Loan.Create(Guid.NewGuid());

        var response = mapper.Map<GetAllLoansResponse>(loan);

        Assert.Equal(loan.Id, response.Id);
        Assert.Equal(loan.BookId, response.BookId);
        Assert.Equal(loan.LoanDate, response.LoanDate);
        Assert.Equal(loan.ReturnDate, response.ReturnDate);
        Assert.Equal(loan.Status, response.Status);
    }
}
