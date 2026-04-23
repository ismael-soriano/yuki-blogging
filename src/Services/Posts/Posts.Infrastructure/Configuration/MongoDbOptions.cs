namespace Posts.Infrastructure.Configuration;

public sealed class MongoDbOptions
{
    public const string SectionName = "MongoDB";

    public string ConnectionString { get; set; } = "mongodb://localhost:27017";

    public string DatabaseName { get; set; } = "posts";
}

