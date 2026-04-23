using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using Posts.Application.Ports;
using Posts.Domain.Abstractions;

namespace Posts.Infrastructure.Persistence;

public sealed class MongoPostEventStore : IPostEventStore
{
    private readonly IMongoCollection<EventStream> eventCollection;

    public MongoPostEventStore(IMongoDatabase database)
    {
        eventCollection = database.GetCollection<EventStream>("event_streams");
    }

    public async Task AppendAsync(Guid streamId, IReadOnlyCollection<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        var storedEvents = domainEvents.Select(ToStoredEvent).ToList();

        var eventStream = new EventStream
        {
            StreamId = streamId,
            Events = storedEvents,
            CreatedAt = DateTimeOffset.UtcNow
        };

        var filter = Builders<EventStream>.Filter.Eq(es => es.StreamId, streamId);
        var existingStream = await eventCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        if (existingStream == null)
        {
            await eventCollection.InsertOneAsync(eventStream, cancellationToken: cancellationToken);
        }
        else
        {
            var update = Builders<EventStream>.Update.PushEach(es => es.Events, storedEvents);
            await eventCollection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
        }
    }

    private static BsonDocument ToStoredEvent(IDomainEvent domainEvent)
    {
        return new BsonDocument
        {
            ["eventType"] = domainEvent.GetType().Name,
            ["occurredOn"] = domainEvent.OccurredOn.UtcDateTime,
            ["payload"] = domainEvent.ToBsonDocument(domainEvent.GetType())
        };
    }

    private sealed class EventStream
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public Guid StreamId { get; set; }

        public List<BsonDocument> Events { get; set; } = new();

        public DateTimeOffset CreatedAt { get; set; }
    }
}


