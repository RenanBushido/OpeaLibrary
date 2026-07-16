namespace OpeaLibrary.Application.Books.Queries.GetBookById;

public sealed record GetBookByIdResponse(
    Guid Id,
    string Title,
    string Author,
    int PublishedYear,
    int QuantityAvailable
);