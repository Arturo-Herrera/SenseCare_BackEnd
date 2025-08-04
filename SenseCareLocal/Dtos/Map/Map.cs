using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
public class Map
{
    [BsonElement("nombreCompleto")]
    public string nombreCompleto { get; set; }

    [BsonElement("sexo")]
    public string Sexo { get; set; }

    [BsonElement("foto")]
    public string foto { get; set; }

    [BsonElement("latitud")]
    public double Latitud { get; set; }

    [BsonElement("longitud")]
    public double Longitud { get; set; }

    [BsonElement("alerta")]
    public Alert? Alerta { get; set; }
}

