using MongoDB.Bson.Serialization.Attributes;

public class RegisterPatientDto
{
    // Datos de Usuario
    [BsonElement("nombre")]
    public string Nombre { get; set; }

    [BsonElement("apellidoPa")]
    public string ApellidoPa { get; set; }

    [BsonElement("apellidoMa")]
    public string ApellidoMa { get; set; }

    [BsonElement("foto")]
    public string Foto { get; set; }

    [BsonElement("fecNac")]
    public DateTime FecNac { get; set; }

    [BsonElement("sexo")]
    public string Sexo { get; set; }

    [BsonElement("dirColonia")]
    public string DirColonia { get; set; }

    [BsonElement("dirCalle")]
    public string DirCalle { get; set; }

    [BsonElement("dirNum")]
    public string DirNum { get; set; }

    [BsonElement("telefono")]
    public string Telefono { get; set; }

    // Referencias
    [BsonElement("IDCuidador")]
    public int IDCuidador { get; set; }

    [BsonElement("IDMedico")]
    public int IDMedico { get; set; }

    [BsonElement("IDDispositivo")]
    public int IDDispositivo { get; set; }
}