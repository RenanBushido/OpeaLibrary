namespace OpeaLibrary.Application.Tests.Books.Commands;

public class AddBookValidatorTests
{
    private readonly AddBookValidator _validator = new();

    [Theory]
    [InlineData("", "Author", 2000)]
    [InlineData(null, "Author", 2000)]
    public async Task Validate_WithInvalidTitle_FailsForTitle(string? title, string author, int publishedYear)
    {
        var result = await _validator.ValidateAsync(new AddBookCommand(title!, author, publishedYear, 1));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddBookCommand.Title));
    }

    [Fact]
    public async Task Validate_WithTitleTooLong_FailsForTitle()
    {
        var command = new AddBookCommand(new string('a', 201), "Author", 2000, 1);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddBookCommand.Title));
    }

    [Theory]
    [InlineData("Title", "", 2000)]
    [InlineData("Title", null, 2000)]
    public async Task Validate_WithInvalidAuthor_FailsForAuthor(string title, string? author, int publishedYear)
    {
        var result = await _validator.ValidateAsync(new AddBookCommand(title, author!, publishedYear, 1));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddBookCommand.Author));
    }

    [Fact]
    public async Task Validate_WithAuthorTooLong_FailsForAuthor()
    {
        var command = new AddBookCommand("Title", new string('a', 101), 2000, 1);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddBookCommand.Author));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(999)]
    [InlineData(3000)]
    public async Task Validate_WithPublishedYearOutOfRange_FailsForPublishedYear(int publishedYear)
    {
        var command = new AddBookCommand("Title", "Author", publishedYear, 1);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddBookCommand.PublishedYear));
    }

    [Fact]
    public async Task Validate_WithNegativeQuantityAvailable_FailsForQuantityAvailable()
    {
        var command = new AddBookCommand("Title", "Author", 2008, -1);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddBookCommand.QuantityAvailable));
    }

    [Fact]
    public async Task Validate_WithValidCommand_Succeeds()
    {
        var command = new AddBookCommand("Clean Code", "Robert C. Martin", 2008, 3);

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
