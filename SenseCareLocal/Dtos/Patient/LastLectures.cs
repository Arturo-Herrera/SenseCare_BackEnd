using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

public class LastLectures
{
    [BsonElement("_id")]
    public int Id { get; set; }

    [BsonElement("pulso")]
    public int[] Pulso { get; set; }

    [BsonElement("temperatura")]
    public double Temperatura { get; set; }

    [BsonElement("oxigeno")]
    public int Oxigeno { get; set; }

    [BsonElement("fecha")]
    public string Fecha { get; set; }

    [BsonElement("fuente")]
    public string Fuente { get; set; }

    [BsonElement("pulsoPromedio")]
    public double PulsoPromedio { get; set; }
}