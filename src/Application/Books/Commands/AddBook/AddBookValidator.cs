namespace OpeaLibrary.Application.Books.Commands.AddBook;

public sealed class AddBookValidator : AbstractValidator<AddBookCommand>
{
    public AddBookValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(100).WithMessage("Author cannot exceed 100 characters.");

        RuleFor(x => x.PublishedYear)
            .NotEmpty().WithMessage("Published year is required.")
            .InclusiveBetween(1000, DateTime.Now.Year).WithMessage("Published year is invalid.");

        RuleFor(x => x.QuantityAvailable)
            .GreaterThanOrEqualTo(0).WithMessage("Quantity available cannot be negative.");
    }
}