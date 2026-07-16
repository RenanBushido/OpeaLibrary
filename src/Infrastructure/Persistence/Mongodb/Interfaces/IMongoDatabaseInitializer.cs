namespace OpeaLibrary.Infrastructure.Persistence.Mongodb.Interfaces;

public interface IMongoDatabaseInitializer
{
    Task EnsureDatabaseCreatedAsync(CancellationToken cancellationToken = default);
}