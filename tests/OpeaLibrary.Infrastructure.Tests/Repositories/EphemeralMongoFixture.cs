namespace OpeaLibrary.Infrastructure.Tests.Repositories;

public sealed class EphemeralMongoFixture : IAsyncLifetime
{
    private static int _guidSerializerRegistered;

    private IMongoRunner? _runner;

    public IMongoClient Client { get; private set; } = null!;

    public Task InitializeAsync()
    {
        // Mirrors OpeaLibrary.CrossCutting.Extensions.InfrastructureExtensions.AddInfraMongo, which
        // registers this once at startup; BsonSerializer.RegisterSerializer throws if called twice,
        // so guard it here since ICollectionFixture only guarantees one fixture instance, not one process-wide call.
        if (Interlocked.Exchange(ref _guidSerializerRegistered, 1) == 0)
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }

        var options = new MongoRunnerOptions
        {
            UseSingleNodeReplicaSet = false,
        };

        _runner = MongoRunner.Run(options);
        Client = new MongoClient(_runner.ConnectionString);

        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        try
        {
            _runner?.Dispose();
        }
        catch (FileNotFoundException)
        {
            // EphemeralMongo.Core 2.0.0 depends on MongoDB.Driver 2.28.0 and its internal
            // "quiet shutdown" path references that version's MongoDB.Driver.Core assembly directly.
            // NuGet unifies the solution on MongoDB.Driver 3.10.0 (this repo's deliberate, src/-wide
            // choice), whose 3.x line no longer ships that assembly, so the graceful shutdown attempt
            // fails to load it. The mongod process itself is still torn down by the runner regardless
            // (MongoRunnerOptions.KillMongoProcessesWhenCurrentProcessExits); this only swallows the
            // failed "ask it nicely first" step, which has no bearing on test correctness.
        }

        return Task.CompletedTask;
    }

    public IMongoDatabase CreateIsolatedDatabase() => Client.GetDatabase($"opealibrary-tests-{Guid.NewGuid():N}");
}

[CollectionDefinition(Name)]
public sealed class EphemeralMongoCollection : ICollectionFixture<EphemeralMongoFixture>
{
    public const string Name = "Ephemeral Mongo";
}
