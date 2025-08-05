using MongoDB.Bson.Serialization.Attributes;

public class HourlyAlerts
{
    [BsonElement("hora")]
    public string Hora { get; set; }

    [BsonElement("totalAlertas")]
    public int TotalAlertas { get; set; }
}

