
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebThree.api;

public class File
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public string Hash { get; set; }

    public string Name { get; set; } = null!;
    public string Category { get; set; } = null!;
    public bool IsPublic { get; set; }
    public string Owner { get; set; } = null!;
    private string EncryptionKey { get; set; } = null!;
}
