using MongoDB.Bson.Serialization.Attributes;

public class AverageVitalsPerDay
{
    [BsonElement("fecha")]
    public string Fecha { get; set; }

    [BsonElement("promedioTemperatura")]
    public double PromedioTemperatura { get; set; }

    [BsonElement("promedioPulso")]
    public double? PromedioPulso { get; set; }
}

