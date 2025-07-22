using MongoDB.Bson.Serialization.Attributes;

public class Treatment
{
    [BsonElement("medicamento")]
    public string Medicine { get; set; }

    [BsonElement("dosis")]
    public string Dosis { get; set; }

    [BsonElement("duracion")]
    public string Duration { get; set; }
}

