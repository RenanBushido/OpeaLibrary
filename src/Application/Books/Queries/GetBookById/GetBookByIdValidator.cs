namespace OpeaLibrary.Application.Books.Queries.GetBookById;

public sealed class GetBookByIdValidator : AbstractValidator<GetBookByIdRequest>
{
    public GetBookByIdValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("The book Id must not be empty.")
            .Must(id => id != Guid.Empty).WithMessage("The book Id must be a valid GUID.");
    }
}