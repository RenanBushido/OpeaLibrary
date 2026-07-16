namespace OpeaLibrary.Api.Endpoints;

public static class BookEndpoint
{
    public static void MapBookEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/books")
            .WithTags("Books");

        group.MapPost("/", CreateBook)
            .WithName("CreateBook")
            .Accepts<CreateBookRequest>("application/json")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

        group.MapGet("/{id}", GetBookById)
            .WithName("GetBookById")
            .Produces<BookResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetAllBooks)
            .WithName("GetAllBooks")
            .Produces<List<BookResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    public static async Task<IResult> CreateBook(
        CreateBookRequest request,
        IMediator mediator
    )
    {
        var command = new AddBookCommand(request.Title, request.Author, request.YearPublished, request.QuantityAvailable);

        var bookId = await mediator.Send(command);

        return Results.Created($"/{bookId}", new { Id = bookId });    
    }

    public static async Task<IResult> GetBookById(
        Guid id,
        IMediator mediator,
        IMapper mapper,
        CancellationToken cancellationToken
    )
    {
        var query = new GetBookByIdRequest(id);

        var book = await mediator.Send(query, cancellationToken);

        if (book is null)
        {
            return Results.NotFound();
        }

        var bookResponse = mapper.Map<BookResponse>(book);

        return Results.Ok(bookResponse);
    }

    public static async Task<IResult> GetAllBooks(
        IMediator mediator,
        CancellationToken cancellationToken
    )
    {

        var request = new GetAllBooksRequest();

        var books = await mediator.Send(request, cancellationToken);

        return Results.Ok(books);
    }

    public sealed record CreateBookRequest(string Title, string Author, int YearPublished, int QuantityAvailable);

    public sealed record BookResponse(Guid Id, string Title, string Author, int YearPublished, int QuantityAvailable);

}