using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
public class UpdateUser
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("nombre")]
    public string Nombre { get; set; }

    [BsonElement("apellidoPa")]
    public string ApellidoPa { get; set; }


    [BsonElement("apellidoMa")]
    public string ApellidoMa { get; set; }

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
    public string? Contrasena { get; set; }

    [BsonElement("activo")]
    public bool Activo { get; set; }

    [BsonElement("IDTipoUsuario")]
    public UserRoleUpdate? IDTipoUsuario { get; set; }
}

public class UserRoleUpdate
{
    [BsonElement("_id")]
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    [BsonElement("descripcion")]
    [JsonPropertyName("descripcion")]
    public string? Descripcion { get; set; }
}
