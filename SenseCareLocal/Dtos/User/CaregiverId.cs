using MongoDB.Bson.Serialization.Attributes;

public class CaregiverId
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("nombreCompleto")]
    public string NombreCompleto { get; set; }
}
