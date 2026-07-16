namespace OpeaLibrary.Application.Books.Queries.GetAllBook;

public sealed record GetAllBookResponse(
    Guid Id,
    string Title,
    string Author,
    int PublishedYear,
    int QuantityAvailable
);