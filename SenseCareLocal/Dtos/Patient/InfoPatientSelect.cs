using MongoDB.Bson.Serialization.Attributes;

public class InfoPatientSelect
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("nombrePaciente")]
    public string FullName { get; set; }
}

