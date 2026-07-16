var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiAutoMapper();
builder.Services.AddApiValidators();
builder.Services.AddApiMediatR();
builder.Services.AddInfraPostgres(builder.Configuration);
builder.Services.AddInfraMongo(builder.Configuration);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
        app.UseSwaggerUI(options =>
            options.SwaggerEndpoint("/openapi/v1.json", "OpeaLibrary.Api v1")
        );

    using(var scope = app.Services.CreateAsyncScope())
    {
        var initializer = scope.ServiceProvider.GetRequiredService<IMongoDatabaseInitializer>();

        await initializer.EnsureDatabaseCreatedAsync();
    }


}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapBookEndpoints();
app.MapLoanEndpoint();

app.Run();