using MongoDB.Bson.Serialization.Attributes;

public class Device
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("fecha")]
    public DateTime Fecha { get; set; }

    [BsonElement("activo")]
    public bool Activo { get; set; }
}
