using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

public class InfoPatientResult
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("paciente")]
    public Usuario Paciente { get; set; }

    [BsonElement("cuidador")]
    public CuidadorInfo Cuidador { get; set; }

    [BsonElement("dispositivo")]
    public DevicePatient Dispositivo { get; set; }
}

public class Usuario
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("nombre")]
    public string Nombre { get; set; }

    [BsonElement("apellidoPa")]
    public string ApellidoPa { get; set; }


    [BsonElement("apellidoMa")]
    public string ApellidoMa { get; set; }

    [BsonElement("foto")]
    public string? Foto { get; set; }

    [BsonElement("fecNac")]
    [JsonConverter(typeof(DateOnlyJsonConverter))]
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

    [BsonElement("email")]
    public string Email { get; set; }

    [BsonElement("contrasena")]
    public string Contrasena { get; set; }

    [BsonElement("activo")]
    public bool Activo { get; set; }

    [BsonElement("IDTipoUsuario")]
    public UserRole IDTipoUsuario { get; set; }

}

public class CuidadorInfo
{
    [BsonElement("_id")]
    public int Id { get; set; }

    [BsonElement("nombre")]
    public string Nombre { get; set; }

    [BsonElement("apellido")]
    public string Apellido { get; set; }

    [BsonElement("telefono")]
    public string Telefono { get; set; }

    [BsonElement("email")]
    public string Email { get; set; }
}
