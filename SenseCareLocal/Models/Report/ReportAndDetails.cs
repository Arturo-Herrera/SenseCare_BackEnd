using MongoDB.Bson.Serialization.Attributes;

public class ReportAndDetails
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("fecha")]
    public DateTime Date { get; set; }

    [BsonElement("motivo")]
    public string Reason { get; set; }

    [BsonElement("diagnostico")]
    public string Diagnosis { get; set; }

    [BsonElement("tratamiento")]
    public Treatment Treatment { get; set; }

    [BsonElement("observaciones")]
    public string Observations { get; set; }

    [BsonElement("IDMedico")]
    public int IDDoctor { get; set; }

    [BsonElement("IDPaciente")]
    public int IDPatient { get; set; }

    [BsonElement("datosAlerta")]
    public Alert[] datosAlerta { get; set; }

    [BsonElement("nombreCompletoMedico")]
    public string NombreCompletoMedico { get; set; }
}

