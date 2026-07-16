namespace OpeaLibrary.Application.Books.Queries.GetAllBook;

public sealed class GetAllBookQueryHandler(
    IBookRepository bookRepository,
    IMapper mapper
) : IRequestHandler<GetAllBookRequest, IEnumerable<GetAllBookResponse>>
{
    private readonly IMapper _mapper = mapper;
    private readonly IBookRepository _bookRepository = bookRepository;

    public async Task<IEnumerable<GetAllBookResponse>> Handle(GetAllBookRequest request, CancellationToken cancellationToken)
    {
        var books = await _bookRepository.GetAllBooksAsync(cancellationToken);
        
        return _mapper.Map<IEnumerable<GetAllBookResponse>>(books);
    }
}