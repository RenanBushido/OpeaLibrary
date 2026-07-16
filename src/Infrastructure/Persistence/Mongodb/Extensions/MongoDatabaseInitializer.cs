namespace OpeaLibrary.Infrastructure.Persistence.Mongodb.Extensions;

public sealed class MongoDatabaseInitializer(IMongoDatabase database, ILogger<MongoDatabaseInitializer> logger) : IMongoDatabaseInitializer
{
    private readonly IMongoDatabase _database = database;
    private readonly ILogger<MongoDatabaseInitializer> _logger = logger;

    private static readonly string[] ExpectedCollections =
    {
        "books",
        "loans"
    };

    public async Task EnsureDatabaseCreatedAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Verifying database '{Database}'...", _database.DatabaseNamespace.DatabaseName);

        var existingCollections = await (await _database.ListCollectionNamesAsync(cancellationToken: cancellationToken))
            .ToListAsync(cancellationToken);

        foreach (var collectionName in ExpectedCollections)
        {
            if (existingCollections.Contains(collectionName))
            {
                _logger.LogInformation("Collection '{Collection}' already exists.", collectionName);
                continue;
            }

            _logger.LogWarning("Collection '{Collection}' not found. Creating...", collectionName);
            
            await _database.CreateCollectionAsync(collectionName, cancellationToken: cancellationToken);
        }

        _logger.LogInformation("Verified database.");
    }
}