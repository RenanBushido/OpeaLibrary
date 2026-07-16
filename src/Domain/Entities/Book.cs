namespace OpeaLibrary.Domain.Entities;

public sealed class Book : Entity
{
    public string Title { get; private set; }
    public string Author { get; private set; }
    public int PublishedYear { get; private set; }
    public int QuantityAvailable { get; private set; }

    public static Book Create(string title, string author, int publishedYear, int quantityAvailable = 0)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("Title cannot be empty.");

        if (string.IsNullOrWhiteSpace(author))
            throw new DomainException("Author cannot be empty.");

        if (publishedYear <= 0)
            throw new DomainException("Published year must be a positive integer.");

        if (quantityAvailable < 0)
            throw new DomainException("Quantity available cannot be negative.");

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = title,
            Author = author,
            PublishedYear = publishedYear,
            QuantityAvailable = quantityAvailable
        };

        return book;
    }

    public void DecreaseQuantity()
    {
        if (QuantityAvailable <= 0)
            throw new DomainException("There are no books available to loan.");

        QuantityAvailable--;
    }

    public void IncreaseQuantity()
    {
        QuantityAvailable++;
    }
}
