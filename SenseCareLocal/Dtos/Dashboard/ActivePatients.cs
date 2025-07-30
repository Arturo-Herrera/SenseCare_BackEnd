using MongoDB.Bson.Serialization.Attributes;

public class ActivePatients
{
    [BsonElement("nombreCompleto")]

    public string nombreCompleto { get; set; }

    public string fecNac { get; set; }

    public string sexo { get; set; }
}
