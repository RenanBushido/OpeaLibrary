namespace OpeaLibrary.Application.Mappings;

public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Book, GetBookByIdResponse>().ReverseMap();
        CreateMap<Book, GetAllBooksResponse>().ReverseMap();
        CreateMap<Loan, GetAllLoansResponse>().ReverseMap();
    }
}