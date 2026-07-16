using Microsoft.Extensions.DependencyInjection;

namespace OpeaLibrary.Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_RegistersMediatRAutoMapperAndValidators()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddApplication();
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetRequiredService<IMediator>());
        Assert.NotNull(provider.GetRequiredService<IMapper>());
        Assert.NotNull(provider.GetRequiredService<IValidator<AddBookCommand>>());
        Assert.NotNull(provider.GetRequiredService<IValidator<GetBookByIdRequest>>());
        Assert.NotNull(provider.GetRequiredService<IValidator<RequestLoanCommand>>());
        Assert.NotNull(provider.GetRequiredService<IValidator<ReturnLoanCommand>>());
    }
}
