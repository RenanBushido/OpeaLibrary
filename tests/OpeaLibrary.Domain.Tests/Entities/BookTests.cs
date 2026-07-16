namespace OpeaLibrary.Domain.Tests.Entities;

public class BookTests
{
    [Fact]
    public void Create_WithValidData_ReturnsBookWithExpectedValues()
    {
        var book = Book.Create("Clean Code", "Robert C. Martin", 2008);

        Assert.NotEqual(Guid.Empty, book.Id);
        Assert.Equal("Clean Code", book.Title);
        Assert.Equal("Robert C. Martin", book.Author);
        Assert.Equal(2008, book.PublishedYear);
        Assert.Equal(0, book.QuantityAvailable);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidTitle_ThrowsDomainException(string? title)
    {
        var exception = Assert.Throws<DomainException>(() => Book.Create(title!, "Author", 2008));

        Assert.Equal("Title cannot be empty.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidAuthor_ThrowsDomainException(string? author)
    {
        var exception = Assert.Throws<DomainException>(() => Book.Create("Title", author!, 2008));

        Assert.Equal("Author cannot be empty.", exception.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositivePublishedYear_ThrowsDomainException(int publishedYear)
    {
        var exception = Assert.Throws<DomainException>(() => Book.Create("Title", "Author", publishedYear));

        Assert.Equal("Published year must be a positive integer.", exception.Message);
    }

    [Fact]
    public void Create_WithNegativeQuantityAvailable_ThrowsDomainException()
    {
        var exception = Assert.Throws<DomainException>(() => Book.Create("Title", "Author", 2008, -1));

        Assert.Equal("Quantity available cannot be negative.", exception.Message);
    }

    [Fact]
    public void Create_WithPositiveQuantityAvailable_SetsQuantityAvailable()
    {
        var book = Book.Create("Title", "Author", 2008, 5);

        Assert.Equal(5, book.QuantityAvailable);
    }

    [Fact]
    public void DecreaseQuantity_WhenLoanAvailable_DecrementsQuantityAvailable()
    {
        var book = Book.Create("Title", "Author", 2008);
        book.IncreaseQuantity();

        book.DecreaseQuantity();

        Assert.Equal(0, book.QuantityAvailable);
    }

    [Fact]
    public void DecreaseQuantity_WhenNoLoanAvailable_ThrowsDomainException()
    {
        var book = Book.Create("Title", "Author", 2008);

        var exception = Assert.Throws<DomainException>(book.DecreaseQuantity);

        Assert.Equal("There are no books available to loan.", exception.Message);
        Assert.Equal(0, book.QuantityAvailable);
    }

    [Fact]
    public void IncreaseQuantity_IncrementsQuantityAvailable()
    {
        var book = Book.Create("Title", "Author", 2008);

        book.IncreaseQuantity();

        Assert.Equal(1, book.QuantityAvailable);
    }
}
