namespace OpeaLibrary.Application.Books.Queries.GetAllBooks;

public sealed class GetAllBooksQueryHandler(
    IBookReadRepository bookRepository,
    IMapper mapper
) : IRequestHandler<GetAllBooksRequest, IEnumerable<GetAllBooksResponse>>
{
    private readonly IMapper _mapper = mapper;
    private readonly IBookReadRepository _bookRepository = bookRepository;

    public async Task<IEnumerable<GetAllBooksResponse>> Handle(GetAllBooksRequest request, CancellationToken cancellationToken)
    {
        var books = await _bookRepository.GetAllBooksAsync(cancellationToken);
        
        return _mapper.Map<IEnumerable<GetAllBooksResponse>>(books);
    }
}