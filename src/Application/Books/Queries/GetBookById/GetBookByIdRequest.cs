namespace OpeaLibrary.Application.Books.Queries.GetBookById;

public sealed record GetBookByIdRequest(Guid Id) : IRequest<GetBookByIdResponse>;