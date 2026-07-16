namespace OpeaLibrary.Api.Endpoints;

public static class LoanEndpoint
{
    public static void MapLoanEndpoint(this WebApplication app)
    {
        var group = app.MapGroup("/api/loans")
            .WithTags("Loans");

        group.MapPut("/request/{id}", RequestLoan)
            .WithName("RequestLoan")            
            .Produces<LoanResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapPut("/return/{id}", ReturnLoan)
            .WithName("ReturnLoan")            
            .Produces<LoanResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);


        group.MapGet("/", GetAllLoans)
            .WithName("GetAllLoans")
            .Produces<List<LoanResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

    }

    public static async Task<IResult> GetAllLoans(
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {
        var query = new GetAllLoansRequest();

        var result = await mediator.Send(query, cancellationToken);

        var loans = result.Select(l => new LoanResponse(l.Id, l.BookId, l.LoanDate, l.ReturnDate, l.Status)).ToList();

        return Results.Ok(loans);
    }

    public static async Task<IResult> ReturnLoan(
        Guid id,
        IMediator mediator
    )
    {
        var command = new ReturnLoanCommand(id);

        var result = await mediator.Send(command);

        return Results.Ok(new { result });
    }

    public static async Task<IResult> RequestLoan(
        Guid id,
        IMediator mediator
    )
    {
        var command = new RequestLoanCommand(id);

        var result = await mediator.Send(command);

        return Results.Ok(new { result });
    }

    public sealed record LoanResponse(
        Guid Id,
        Guid BookId,
        DateTime LoanDate,
        DateTime? ReturnDate,
        StatusLoan Status);
}