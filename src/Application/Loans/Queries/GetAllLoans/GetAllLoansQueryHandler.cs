namespace OpeaLibrary.Application.Loans.Queries.GetAllLoans;

public sealed class GetAllLoansQueryHandler(ILoanRepository loanRepository, IMapper mapper) 
    : IRequestHandler<GetAllLoansRequest, IEnumerable<GetAllLoansResponse>>
{
    private readonly ILoanRepository _loanRepository = loanRepository;
    private readonly IMapper _mapper = mapper;
    

    public async Task<IEnumerable<GetAllLoansResponse>> Handle(GetAllLoansRequest request, CancellationToken cancellationToken)
    {
        var loans = await _loanRepository.GetAllLoansAsync(cancellationToken);
        
        return _mapper.Map<IEnumerable<GetAllLoansResponse>>(loans);
    }
}