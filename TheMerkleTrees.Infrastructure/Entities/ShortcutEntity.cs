using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TheMerkleTrees.Infrastructure.Entities;

public class ShortcutEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string Owner { get; set; } = null!;
}