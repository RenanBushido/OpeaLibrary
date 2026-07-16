namespace OpeaLibrary.Application.Books.Queries.GetAllBooks;

public sealed record GetAllBooksResponse(
    Guid Id,
    string Title,
    string Author,
    int PublishedYear,
    int QuantityAvailable
);