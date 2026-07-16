namespace OpeaLibrary.CrossCutting.Tests.Extensions;

public class InfrastructureExtensionsTests
{
    private static IConfiguration BuildConfiguration(string key, string? value)
    {
        var data = new Dictionary<string, string?>();
        if (value is not null)
        {
            data[$"ConnectionStrings:{key}"] = value;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(data).Build();
    }

    private static bool HasScopedRegistration(IServiceCollection services, Type serviceType, Type implementationType) =>
        services.Any(d =>
            d.ServiceType == serviceType &&
            d.ImplementationType == implementationType &&
            d.Lifetime == ServiceLifetime.Scoped);

    [Fact]
    public void AddInfraPostgres_RegistersWriteRepositoriesAndUnitOfWork()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration("OpeaLibraryWriteConnection", "Host=placeholder;Database=placeholder;Username=placeholder;Password=placeholder");

        services.AddInfraPostgres(configuration);

        Assert.True(HasScopedRegistration(services, typeof(IBookWriteRepository), typeof(BookWriteRepository)));
        Assert.True(HasScopedRegistration(services, typeof(ILoanWriteRepository), typeof(LoanWriteRepository)));
        Assert.True(HasScopedRegistration(services, typeof(IUnitOfWork), typeof(UnitOfWork)));
    }

    [Fact]
    public void AddInfraPostgres_WhenWriteConnectionStringIsMissing_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration("OpeaLibraryWriteConnection", null);

        Assert.Throws<InvalidOperationException>(() => services.AddInfraPostgres(configuration));
    }

    [Fact]
    public void AddInfraMongo_RegistersReadRepositoriesAndInitializer()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration("OpeaLibraryReadConnection", "mongodb://placeholder:27017");

        services.AddInfraMongo(configuration);

        Assert.True(HasScopedRegistration(services, typeof(IBookReadRepository), typeof(BookReadRepository)));
        Assert.True(HasScopedRegistration(services, typeof(ILoanReadRepository), typeof(LoanReadRepository)));
        Assert.True(HasScopedRegistration(services, typeof(IMongoDatabaseInitializer), typeof(MongoDatabaseInitializer)));
    }

    [Fact]
    public void AddInfraMongo_WhenReadConnectionStringIsMissing_ThrowsInvalidOperationException()
    {
        var services = new ServiceCollection();
        var configuration = BuildConfiguration("OpeaLibraryReadConnection", null);

        Assert.Throws<InvalidOperationException>(() => services.AddInfraMongo(configuration));
    }
}
