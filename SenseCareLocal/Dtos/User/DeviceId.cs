using MongoDB.Bson.Serialization.Attributes;

public class DeviceId
{
    [BsonId]
    public int Id { get; set; }
}
