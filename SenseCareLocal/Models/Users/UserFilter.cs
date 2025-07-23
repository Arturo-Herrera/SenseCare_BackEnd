using MongoDB.Bson.Serialization.Attributes;

public class UserFilter
{
    public bool? Active { get; set; }

    public string? Role { get; set; }

}

public class UserFilterData
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("nombreCompleto")]
    public string FullName { get; set; }

    [BsonElement("fecNac")]
    public DateTime BirthDate { get; set; }

    [BsonElement("sexo")]
    public string Gender { get; set; }

    [BsonElement("direccionCompleta")]
    public string FullAddress { get; set; }

    [BsonElement("telefono")]
    public string PhoneNumber { get; set; }

    [BsonElement("email")]
    public string Email { get; set; }

    [BsonElement("activo")]
    public bool Active { get; set; }

    [BsonElement("IDTipoUsuario")]
    public string IDUserType { get; set; }

    [BsonElement("DescripcionTipoUsuario")]
    public string DescriptionUserType { get; set; }
} 

