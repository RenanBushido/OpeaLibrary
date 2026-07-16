namespace OpeaLibrary.Application.Books.Commands.AddBook;

public sealed class AddBookCommandHandler(IUnitOfWork unitOfWork)
     : IRequestHandler<AddBookCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Guid> Handle(AddBookCommand request, CancellationToken cancellationToken)
    {
        var book = Book.Create(request.Title, request.Author, request.PublishedYear, request.QuantityAvailable);

        await _unitOfWork.BookWriteRepository.AddBookAsync(book);

        await _unitOfWork.CommitAsync(cancellationToken);

        return book.Id;
    }
}