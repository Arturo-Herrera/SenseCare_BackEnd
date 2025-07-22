using MongoDB.Bson.Serialization.Attributes;

public class InfoPatientSelect
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("nombreCompleto")]
    public string FullName { get; set; }
}

