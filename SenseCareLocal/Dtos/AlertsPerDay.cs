using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson.Serialization.Attributes;

public class AlertsPerDay
{
    [BsonElement("fecha")]
    public string Fecha { get; set; }

    [BsonElement("totalAlertas")]
    public int TotalAlertas { get; set; }
}

