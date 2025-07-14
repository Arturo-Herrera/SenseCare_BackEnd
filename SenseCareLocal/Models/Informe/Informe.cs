using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SenseCareLocal.Models.Informe;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class Informe
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("fecha")]
    [JsonConverter(typeof(DateOnlyJsonConverter))] 
    public DateTime Fecha { get; set; }

    [BsonElement("motivo")]
    public string Motivo { get; set; }

    [BsonElement("diagnostico")]
    public string Diagnostico { get; set; }

    [BsonElement("tratamiento")]
    public Tratamiento Tratamiento { get; set; }

    [BsonElement("observaciones")]
    public string Observaciones { get; set; }

    [BsonElement("IDMedico")]
    public int IDMedico { get; set; }

    [BsonElement("IDAlertas")]
    public List<int> IDAlertas { get; set; }
}
