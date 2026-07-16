namespace OpeaLibrary.Application.Books.Queries.GetAllBooks;

public sealed record GetAllBooksRequest : IRequest<IEnumerable<GetAllBooksResponse>>;