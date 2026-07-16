namespace OpeaLibrary.CrossCutting.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApiMediatR(this IServiceCollection services)
    {
        var opeaHandlers = AppDomain.CurrentDomain.Load("OpeaLibrary.Application");

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(opeaHandlers);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });
        

        return services;
    }

    public static IServiceCollection AddApiAutoMapper(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        return services;
    }

    public static IServiceCollection AddApiValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<AddBookCommand>, AddBookValidator>();
        services.AddScoped<IValidator<GetBookByIdRequest>, GetBookByIdValidator>();
        services.AddScoped<IValidator<RequestLoanCommand>, RequestLoanValidator>();
        services.AddScoped<IValidator<ReturnLoanCommand>, ReturnLoanValidator>();

        return services;
    }
}