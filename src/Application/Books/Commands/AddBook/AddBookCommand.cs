namespace OpeaLibrary.Application.Books.Commands.AddBook;

public sealed record AddBookCommand(string Title, string Author, int PublishedYear, int QuantityAvailable) : IRequest<Guid>;