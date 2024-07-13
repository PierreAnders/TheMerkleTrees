using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class UserEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("Email")]
    public string Email { get; set; }

    [BsonElement("PasswordHash")]
    public string PasswordHash { get; set; }
}