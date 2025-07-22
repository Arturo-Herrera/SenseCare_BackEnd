using MongoDB.Bson.Serialization.Attributes;

public class LastAlerts
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("signoAfectado")]
    public string SignoAfectado { get; set; }

    [BsonElement("tipoAlerta")]
    public AlertType AlertType { get; set; }

    [BsonElement("fecha")]
    public string Date { get; set; }

    [BsonElement("paciente")]
    public PatientLastAlert Patient { get; set;}
}

public class AlertType 
{
    [BsonElement("descripcion")]
    public string Description { get; set; }
}

public class PatientLastAlert 
{
    [BsonId]
    public int Id { get; set; }

    [BsonElement("nombre")]
    public string Name { get; set; }
}


