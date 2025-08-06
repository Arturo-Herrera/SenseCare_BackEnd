using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Bson.Serialization.Attributes;

public class AlertsPerDay
{
    [BsonElement("promedioAlertasPorDia")]
    public double promedioAlertasPorDia { get; set; }

}
