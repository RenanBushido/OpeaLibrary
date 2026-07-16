namespace OpeaLibrary.Application.Tests.Books.Queries;

public class GetBookByIdValidatorTests
{
    private readonly GetBookByIdValidator _validator = new();

    [Fact]
    public async Task Validate_WithEmptyGuid_Fails()
    {
        var result = await _validator.ValidateAsync(new GetBookByIdRequest(Guid.Empty));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetBookByIdRequest.Id));
    }

    [Fact]
    public async Task Validate_WithNonEmptyGuid_Succeeds()
    {
        var result = await _validator.ValidateAsync(new GetBookByIdRequest(Guid.NewGuid()));

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }
}
