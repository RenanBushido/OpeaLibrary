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
    public void Create_WithInvalidTitle_ThrowsArgumentException(string? title)
    {
        var exception = Assert.Throws<ArgumentException>(() => Book.Create(title!, "Author", 2008));

        Assert.Equal("title", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidAuthor_ThrowsArgumentException(string? author)
    {
        var exception = Assert.Throws<ArgumentException>(() => Book.Create("Title", author!, 2008));

        Assert.Equal("author", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_WithNonPositivePublishedYear_ThrowsArgumentException(int publishedYear)
    {
        var exception = Assert.Throws<ArgumentException>(() => Book.Create("Title", "Author", publishedYear));

        Assert.Equal("publishedYear", exception.ParamName);
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
    public void DecreaseQuantity_WhenNoLoanAvailable_ThrowsInvalidOperationException()
    {
        var book = Book.Create("Title", "Author", 2008);

        Assert.Throws<InvalidOperationException>(book.DecreaseQuantity);
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
