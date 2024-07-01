using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TheMerkleTrees.Infrastructure.Entities;

public class FileEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string Hash { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public bool IsPublic { get; set; }
    public string Owner { get; set; } = null!;
    public string EncryptionKey { get; set; } = null!;
}