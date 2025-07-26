using MongoDB.Bson.Serialization.Attributes;

public class CaregiverId
{
    [BsonId]
    public int Id { get; set; }
}

