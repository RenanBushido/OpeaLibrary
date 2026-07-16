namespace OpeaLibrary.Api.Exceptions;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger
) : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService = problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Unhandled exception {ExceptionType}", exception.GetType().Name);

        int statusCode = exception is DomainException domainException
            ? StatusCodes.Status400BadRequest
            : exception is ValidationException ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;
        httpContext.Response.StatusCode = statusCode;

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = GetTitle(statusCode),
                Detail = exception.Message
            }
        });
    }

    private static string GetTitle(int statusCode) =>
        statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status417ExpectationFailed => "Expectation failed",
            StatusCodes.Status500InternalServerError => "Internal Server Error",
            _ => "Error"
        };
}