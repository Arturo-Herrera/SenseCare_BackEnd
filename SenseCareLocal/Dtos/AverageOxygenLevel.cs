using MongoDB.Bson.Serialization.Attributes;

public class AverageOxygenLevel
{
    [BsonElement("promedioOxigeno")]
    public int PromedioOxigeno { get; set; }
}

