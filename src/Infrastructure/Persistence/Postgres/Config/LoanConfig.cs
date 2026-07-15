namespace OpeaLibrary.Infrastructure.Persistence.Postgres.Config;

public class LoanConfig : IEntityTypeConfiguration<Loan>
{
    public void Configure(EntityTypeBuilder<Loan> builder)
    {
        builder.ToTable("tb_loans");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Id)
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(l => l.BookId)
            .HasColumnType("uuid")
            .IsRequired();        

        builder.Property(l => l.LoanDate)
            .IsRequired();

        builder.Property(l => l.ReturnDate)
            .IsRequired(false);
    }
}