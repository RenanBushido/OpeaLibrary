namespace OpeaLibrary.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

        services.AddScoped<IValidator<AddBookCommand>, AddBookValidator>();
        services.AddScoped<IValidator<GetBookByIdRequest>, GetBookByIdValidator>();
        services.AddScoped<IValidator<RequestLoanCommand>, RequestLoanValidator>();
        services.AddScoped<IValidator<ReturnLoanCommand>, ReturnLoanValidator>();

        return services;
    }
}
