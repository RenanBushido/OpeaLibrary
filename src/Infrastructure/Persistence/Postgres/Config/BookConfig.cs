namespace OpeaLibrary.Infrastructure.Persistence.Postgres.Config;

public class BookConfig : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("tb_books");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Id)
            .HasColumnType("uuid")
            .IsRequired();            

        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Author)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(b => b.PublishedYear)
            .IsRequired();

        builder.Property(b => b.QuantityAvailable)
            .IsRequired();
    }
}