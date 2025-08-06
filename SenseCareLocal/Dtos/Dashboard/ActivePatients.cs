using MongoDB.Bson.Serialization.Attributes;

public class ActivePatients
{
    [BsonElement("pacientesActivos")]
    public int PacientesActivos { get; set; }
}
