using MongoDB.Bson.Serialization.Attributes;

public class AvgManual
{
    [BsonElement("promedioSignosPorDia")]
    public double PromedioSignosPorDia { get; set; }
}

