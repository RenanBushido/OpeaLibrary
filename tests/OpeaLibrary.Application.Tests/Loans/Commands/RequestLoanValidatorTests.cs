namespace OpeaLibrary.Application.Tests.Loans.Commands;

public class RequestLoanValidatorTests
{
    private readonly RequestLoanValidator _validator = new();

    [Fact]
    public async Task Validate_WithEmptyGuid_Fails()
    {
        var result = await _validator.ValidateAsync(new RequestLoanCommand(Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(RequestLoanCommand.BookId));
    }

    [Fact]
    public async Task Validate_WithNonEmptyGuid_Succeeds()
    {
        var result = await _validator.ValidateAsync(new RequestLoanCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
