namespace OpeaLibrary.Application.Books.Queries.GetAllBook;

public sealed record GetAllBookRequest() : IRequest<IEnumerable<GetAllBookResponse>>;