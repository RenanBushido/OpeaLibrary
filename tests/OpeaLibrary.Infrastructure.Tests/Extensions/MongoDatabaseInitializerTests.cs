using OpeaLibrary.Infrastructure.Tests.Repositories;

namespace OpeaLibrary.Infrastructure.Tests.Extensions;

[Collection(EphemeralMongoCollection.Name)]
public class MongoDatabaseInitializerTests(EphemeralMongoFixture fixture)
{
    [Fact]
    public async Task EnsureDatabaseCreatedAsync_WhenNoCollectionsExist_CreatesBooksAndLoansCollections()
    {
        var database = fixture.CreateIsolatedDatabase();
        var sut = new MongoDatabaseInitializer(database, NullLogger<MongoDatabaseInitializer>.Instance);

        await sut.EnsureDatabaseCreatedAsync(CancellationToken.None);

        var collectionNames = await (await database.ListCollectionNamesAsync()).ToListAsync();
        Assert.Contains("books", collectionNames);
        Assert.Contains("loans", collectionNames);
    }

    [Fact]
    public async Task EnsureDatabaseCreatedAsync_WhenCollectionsAlreadyExist_DoesNotThrow()
    {
        var database = fixture.CreateIsolatedDatabase();
        await database.CreateCollectionAsync("books");
        await database.CreateCollectionAsync("loans");
        var sut = new MongoDatabaseInitializer(database, NullLogger<MongoDatabaseInitializer>.Instance);

        await sut.EnsureDatabaseCreatedAsync(CancellationToken.None);

        var collectionNames = await (await database.ListCollectionNamesAsync()).ToListAsync();
        Assert.Contains("books", collectionNames);
        Assert.Contains("loans", collectionNames);
    }
}
