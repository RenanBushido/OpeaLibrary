namespace OpeaLibrary.CrossCutting.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfraPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OpeaLibraryWriteConnection")
            ?? throw new InvalidOperationException("Connection string 'OpeaLibraryConnection' not found.");

        services.AddScoped<DomainEventsInterceptor>();

        services.AddDbContext<OpeaLibraryDbContext>((serviceProvider, options) =>
            options.UseNpgsql(connectionString)
                .AddInterceptors(serviceProvider.GetRequiredService<DomainEventsInterceptor>()));

        services.AddSingleton<IDbConnection>(provider =>
        {
            var connecton = new NpgsqlConnection(connectionString);

            if (connecton.State != ConnectionState.Open) connecton.Open();

            return connecton;
        });

        services.AddScoped<IBookWriteRepository, BookWriteRepository>();
        services.AddScoped<ILoanWriteRepository, LoanWriteRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }

    public static IServiceCollection AddInfraMongo(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("OpeaLibraryReadConnection")
            ?? throw new InvalidOperationException("Connection string 'OpeaLibraryReadConnection' not found.");

        services.AddSingleton<IMongoClient>(cfg =>
        {
            var settings = MongoClientSettings.FromConnectionString(connectionString);

            return new MongoClient(settings);
        });

        services.AddScoped(sp => sp.GetRequiredService<IMongoClient>().GetDatabase("OpenLibrary"));


        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

        services.AddScoped<IBookReadRepository, BookReadRepository>();
        services.AddScoped<ILoanReadRepository, LoanReadRepository>();

        services.AddScoped<IMongoDatabaseInitializer, MongoDatabaseInitializer>();
        

        return services;
    }
}