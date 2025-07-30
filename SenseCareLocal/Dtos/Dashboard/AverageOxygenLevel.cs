using MongoDB.Bson.Serialization.Attributes;

public class AverageOxygenLevel
{
    [BsonElement("promedioOxigeno")]
    public double promedioOxigeno { get; set; }
}
