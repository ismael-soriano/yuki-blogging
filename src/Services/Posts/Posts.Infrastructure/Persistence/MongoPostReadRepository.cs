using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Posts.Application;
using Posts.Application.Ports;

namespace Posts.Infrastructure.Persistence;

public sealed class MongoPostReadRepository : IPostReadRepository
{
    private readonly IMongoCollection<PostReadModelDb> postCollection;

    public MongoPostReadRepository(IMongoDatabase database)
    {
        postCollection = database.GetCollection<PostReadModelDb>("posts");
    }

    public async Task<PostReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var filter = Builders<PostReadModelDb>.Filter.Eq(p => p.Id, id);
        var post = await postCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);

        return post == null ? null : post.ToModel();
    }

    public async Task<IReadOnlyList<PostReadModel>> GetAllAsync(int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var skip = (page - 1) * pageSize;

        var posts = await postCollection
            .Find(Builders<PostReadModelDb>.Filter.Empty)
            .SortByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        return posts.Select(p => p.ToModel()).ToList();
    }

    public async Task SaveAsync(PostReadModel post, CancellationToken cancellationToken)
    {
        var dbPost = new PostReadModelDb
        {
            MongoId = ObjectId.GenerateNewId(),
            Id = post.Id,
            AuthorId = post.AuthorId,
            Title = post.Title,
            Description = post.Description,
            Content = post.Content,
            IsDeleted = post.IsDeleted,
        };

        await postCollection.InsertOneAsync(dbPost, cancellationToken: cancellationToken);
    }

    public async Task UpdateAsync(PostReadModel post, CancellationToken cancellationToken)
    {
        var filter = Builders<PostReadModelDb>.Filter.Eq(p => p.Id, post.Id);
        var update = Builders<PostReadModelDb>.Update
            .Set(p => p.Title, post.Title)
            .Set(p => p.Description, post.Description)
            .Set(p => p.Content, post.Content)
            .Set(p => p.IsDeleted, post.IsDeleted)
            .Set(p => p.UpdatedAt, DateTimeOffset.UtcNow);

        await postCollection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var filter = Builders<PostReadModelDb>.Filter.Eq(p => p.Id, id);
        var update = Builders<PostReadModelDb>.Update.Set(p => p.IsDeleted, true);

        await postCollection.UpdateOneAsync(filter, update, cancellationToken: cancellationToken);
    }

    private sealed class PostReadModelDb
    {
        [BsonId]
        public ObjectId MongoId { get; set; }

        [BsonElement("Id")]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid Id { get; set; }

        [BsonElement("AuthorId")]
        [BsonGuidRepresentation(GuidRepresentation.Standard)]
        public Guid AuthorId { get; set; }

        public string Title { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        public string Content { get; set; } = string.Empty;
        
        public bool IsDeleted { get; set; }
        
        public DateTimeOffset CreatedAt { get; set; }
        
        public DateTimeOffset? UpdatedAt { get; set; }

        public PostReadModel ToModel() => new(Id, AuthorId, Title, Description, Content, IsDeleted);
    }
}

