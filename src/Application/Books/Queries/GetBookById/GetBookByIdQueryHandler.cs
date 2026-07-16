namespace OpeaLibrary.Application.Books.Queries.GetBookById;

public sealed class GetBookByIdQueryHandler(IBookRepository bookRepository, IMapper mapper)
    : IRequestHandler<GetBookByIdRequest, GetBookByIdResponse>
{
    private readonly IBookRepository _bookRepository = bookRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<GetBookByIdResponse> Handle(GetBookByIdRequest request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetBookByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Book with id '{request.Id}' was not found.");

        return _mapper.Map<GetBookByIdResponse>(book);
    }
}