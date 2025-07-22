using MongoDB.Bson.Serialization.Attributes;

public class AverageVitalSign
{
    [BsonElement("IDPaciente")]
    public int IDPatient { get; set; }

    [BsonElement("dia")]
    public DateTime Date { get; set; }

    [BsonElement("promedioPulso")]
    public double AveragePulse { get; set; }

    [BsonElement("promedioTemperatura")]
    public double AverageTemperature { get; set; }

    [BsonElement("promedioOxigeno")]
    public double AverageOxygen { get; set; }

}
