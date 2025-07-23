using MongoDB.Bson.Serialization.Attributes;

public class DevicePatient
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("fecha")]
    public DateTime Fecha { get; set; }

    [BsonElement("activo")]
    public bool Activo { get; set; }

    [BsonElement("latitud")]
    public double Latitude { get; set; }

    [BsonElement("longitud")]
    public double Longitude { get; set; }
}
