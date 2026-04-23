const database = db.getSiblingDB("posts");

const postId = UUID("11111111-1111-1111-1111-111111111111");
const authorId = UUID("9f9df8ca-4314-4d0d-a629-fcb0cead5dae");
const now = new Date();

// Create indexes and seed read model data idempotently.
database.posts.createIndex({ Id: 1 }, { unique: true });
database.event_streams.createIndex({ StreamId: 1 }, { unique: true });

if (database.posts.countDocuments({ Id: postId }) === 0) {
  database.posts.insertOne({
    Id: postId,
    IdText: "11111111-1111-1111-1111-111111111111",
    AuthorId: authorId,
    Title: "Seeded post",
    Description: "Created by docker-compose MongoDB initialization",
    Content: "This document is inserted at container bootstrap.",
    IsDeleted: false,
    CreatedAt: now,
    UpdatedAt: null,
  });
}

if (database.event_streams.countDocuments({ StreamId: postId }) === 0) {
  database.event_streams.insertOne({
    StreamId: postId,
    Events: [
      {
        Id: postId,
        OccurredOnUtc: now,
        AuthorId: authorId,
        Title: "Seeded post",
        Description: "Created by docker-compose MongoDB initialization",
        Content: "This document is inserted at container bootstrap.",
      },
    ],
    CreatedAt: now,
  });
}


