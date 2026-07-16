namespace OpeaLibrary.Application.Tests.Behaviors;

public class ValidationBehaviorTests
{
    public sealed record TestRequest(string Value);

    [Fact]
    public async Task Handle_WithPassingValidator_InvokesNextOnce()
    {
        var validator = new Mock<IValidator<TestRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var behavior = new ValidationBehavior<TestRequest, string>([validator.Object]);
        var nextCallCount = 0;
        Task<string> Next(CancellationToken _)
        {
            nextCallCount++;
            return Task.FromResult("ok");
        }

        var result = await behavior.Handle(new TestRequest("value"), Next, CancellationToken.None);

        Assert.Equal("ok", result);
        Assert.Equal(1, nextCallCount);
    }

    [Fact]
    public async Task Handle_WithFailingValidator_ThrowsAndDoesNotInvokeNext()
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>
        {
            new(nameof(TestRequest.Value), "Value is invalid.")
        };
        var validator = new Mock<IValidator<TestRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(failures));

        var behavior = new ValidationBehavior<TestRequest, string>([validator.Object]);
        var nextCallCount = 0;
        Task<string> Next(CancellationToken _)
        {
            nextCallCount++;
            return Task.FromResult("ok");
        }

        await Assert.ThrowsAsync<ValidationException>(() => behavior.Handle(new TestRequest("value"), Next, CancellationToken.None));
        Assert.Equal(0, nextCallCount);
    }

    [Fact]
    public async Task Handle_WithNoRegisteredValidator_InvokesNextOnce()
    {
        var behavior = new ValidationBehavior<TestRequest, string>([]);
        var nextCallCount = 0;
        Task<string> Next(CancellationToken _)
        {
            nextCallCount++;
            return Task.FromResult("ok");
        }

        var result = await behavior.Handle(new TestRequest("value"), Next, CancellationToken.None);

        Assert.Equal("ok", result);
        Assert.Equal(1, nextCallCount);
    }
}
