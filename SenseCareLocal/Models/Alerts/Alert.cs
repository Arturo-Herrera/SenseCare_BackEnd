using MongoDB.Bson.Serialization.Attributes;

public class Alert
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("fecha")]
    public DateTime Fecha { get; set; }

    [BsonElement("signoAfectado")]
    public string SignoAfectado { get; set; }

    [BsonElement("IDPaciente")]
    public int IDPaciente { get; set; }

    [BsonElement("IDTipoAlerta")]
    public string IDTipoAlerta { get; set; }
}

