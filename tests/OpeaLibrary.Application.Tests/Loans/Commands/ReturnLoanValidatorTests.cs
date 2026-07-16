namespace OpeaLibrary.Application.Tests.Loans.Commands;

public class ReturnLoanValidatorTests
{
    private readonly ReturnLoanValidator _validator = new();

    [Fact]
    public async Task Validate_WithEmptyGuid_Fails()
    {
        var result = await _validator.ValidateAsync(new ReturnLoanCommand(Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReturnLoanCommand.LoanId));
    }

    [Fact]
    public async Task Validate_WithNonEmptyGuid_Succeeds()
    {
        var result = await _validator.ValidateAsync(new ReturnLoanCommand(Guid.NewGuid()));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
