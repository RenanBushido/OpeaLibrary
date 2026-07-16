namespace OpeaLibrary.CrossCutting.Tests.Extensions;

public class ApplicationExtensionsTests
{
    [Fact]
    public void AddApiMediatR_RegistersMediatorAndValidationBehavior()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddApiMediatR();

        Assert.Contains(
            services,
            d => d.ServiceType == typeof(IPipelineBehavior<,>) && d.ImplementationType == typeof(ValidationBehavior<,>));

        using var provider = services.BuildServiceProvider();
        var mediator = provider.GetService<IMediator>();

        Assert.NotNull(mediator);
    }

    [Fact]
    public void AddApiMediatR_RegistersInfrastructureNotificationHandlers()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddApiMediatR();

        Assert.Contains(
            services,
            d => d.ServiceType == typeof(INotificationHandler<DomainEventNotification<BookCreatedEvent>>) && d.ImplementationType == typeof(BookCreatedEventHandler));
        Assert.Contains(
            services,
            d => d.ServiceType == typeof(INotificationHandler<DomainEventNotification<BookQuantityChangedEvent>>) && d.ImplementationType == typeof(BookQuantityChangedEventHandler));
        Assert.Contains(
            services,
            d => d.ServiceType == typeof(INotificationHandler<DomainEventNotification<LoanCreatedEvent>>) && d.ImplementationType == typeof(LoanCreatedEventHandler));
        Assert.Contains(
            services,
            d => d.ServiceType == typeof(INotificationHandler<DomainEventNotification<LoanReturnedEvent>>) && d.ImplementationType == typeof(LoanReturnedEventHandler));
    }

    [Fact]
    public void AddApiAutoMapper_RegistersMappingProfile()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        services.AddApiAutoMapper();

        using var provider = services.BuildServiceProvider();
        var mapper = provider.GetService<IMapper>();

        Assert.NotNull(mapper);
        mapper!.ConfigurationProvider.AssertConfigurationIsValid();
    }

    [Fact]
    public void AddApiValidators_RegistersAllFourValidatorsWithScopedLifetime()
    {
        var services = new ServiceCollection();

        services.AddApiValidators();

        using var provider = services.BuildServiceProvider();

        Assert.IsType<AddBookValidator>(provider.GetService<IValidator<AddBookCommand>>());
        Assert.IsType<GetBookByIdValidator>(provider.GetService<IValidator<GetBookByIdRequest>>());
        Assert.IsType<RequestLoanValidator>(provider.GetService<IValidator<RequestLoanCommand>>());
        Assert.IsType<ReturnLoanValidator>(provider.GetService<IValidator<ReturnLoanCommand>>());

        Assert.All(
            services.Where(d => d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == typeof(IValidator<>)),
            d => Assert.Equal(ServiceLifetime.Scoped, d.Lifetime));
    }
}
