namespace OpeaLibrary.Infrastructure.Persistence.Mongodb.Mappings;

public static class MongoMappings
{
    public static void Configure()
    {
        BsonClassMap.RegisterClassMap<Book>(cm =>
        {
            cm.AutoMap();
            cm.MapIdProperty(b => b.Id)
            .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
            cm.SetIgnoreExtraElements(true);
        });
    }
}