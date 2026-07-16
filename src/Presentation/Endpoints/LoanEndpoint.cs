namespace OpeaLibrary.Api.Endpoints;

public static class LoanEndpoint
{
    public static void MapLoanEndpoint(this WebApplication app)
    {
        var group = app.MapGroup("/api/loans")
            .WithTags("Loans");

        group.MapPost("/request", RequestLoan)
            .WithName("RequestLoan")
            .Accepts<LoanRequest>("application/json")
            .Produces<LoanResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPost("/return", ReturnLoan)
            .WithName("ReturnLoan")
            .Accepts<LoanRequest>("application/json")
            .Produces<LoanResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);


        group.MapGet("/", GetAllLoans)
            .WithName("GetAllLoans")
            .Produces<List<LoanResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    }

    public static async Task<IResult> GetAllLoans(
        IMediator mediator,
        IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var command = new GetAllBooksRequest();

        var result = await mediator.Send(command, cancellationToken);

        var loans = mapper.Map<List<LoanResponse>>(result);

        return Results.Ok(loans);
    }

    public static async Task<IResult> ReturnLoan(
        LoanRequest request,
        IMediator mediator
    )
    {
        var command = new RequestLoanCommand(request.BookId);

        var result = await mediator.Send(command);

        return Results.Created($"/{result}", new { result });
    }

    public static async Task<IResult> RequestLoan(
        LoanRequest request,
        IMediator mediator
    )
    {
        var command = new RequestLoanCommand(request.BookId);

        var result = await mediator.Send(command);

        return Results.Created($"/{result}", new { result });
    }

    public sealed record LoanRequest(Guid BookId);
    public sealed record LoanResponse(
        Guid Id,
        Guid BookId,
        DateTime LoanDate,
        DateTime? ReturnDate,
        StatusLoan Status);
}