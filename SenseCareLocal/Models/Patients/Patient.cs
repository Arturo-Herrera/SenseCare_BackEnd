using MongoDB.Bson.Serialization.Attributes;

public class Patient
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("IDUsuario")]
    public int IDUsuario { get; set; }

    [BsonElement("IDCuidador")]
    public int IDCuidador { get; set; }

    [BsonElement("IDMedico")]
    public int IDMedico { get; set; }

    [BsonElement("IDDispositivo")]
    public int IDDispositivo { get; set; }
}
