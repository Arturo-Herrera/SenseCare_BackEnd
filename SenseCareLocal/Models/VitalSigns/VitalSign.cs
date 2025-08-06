using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class VitalSign
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("fecha")]
    public DateTime Fecha { get; set; }

    [BsonElement("IDPaciente")]
    public int IDPaciente { get; set; }

    [BsonElement("fuente")]
    public bool Fuente { get; set; }

    [BsonElement("pulso")]
    public List<int> Pulso { get; set; } = new();

    [BsonElement("temperatura")]
    public double Temperatura { get; set; }

    [BsonElement("oxigeno")]
    public double Oxigeno { get; set; }
}