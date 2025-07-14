using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson.Serialization.Attributes;

public class UserRole
{
    [BsonElement("_id")]
    [JsonPropertyName("_id")]
    public string Id { get; set; }

    [BsonElement("descripcion")]
    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; }
}