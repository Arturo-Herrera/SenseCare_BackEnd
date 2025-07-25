using MongoDB.Bson.Serialization.Attributes;

public class Device
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("fecha")]
    public DateTime Fecha { get; set; }

    [BsonElement("latitud")]
    public double Latitud { get; set; }

    [BsonElement("longitud")]
    public double Longitud { get; set; }

    [BsonElement("activo")]
    public bool Activo { get; set; }
}
