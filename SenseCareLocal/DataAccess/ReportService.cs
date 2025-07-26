using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SenseCareLocal.Config;

public class ReportService
{
    private readonly IMongoCollection<Report> _report;

    public ReportService(IOptions<MongoDBSettings> config)
    {
        var client = new MongoClient(config.Value.ConnectionString);
        var database = client.GetDatabase(config.Value.DatabaseName);
        _report = database.GetCollection<Report>("Informe");
    }

    public async Task<List<ReportAndDetails>> GetAllByPatient(int idPatient)
    {
        var reports = $@"
            [
  {{
    $lookup: {{
      from: ""Paciente"",
      localField: ""IDPaciente"",
      foreignField: ""_id"",
      as: ""datosPaciente""
    }}
  }},
  {{ $unwind: ""$datosPaciente"" }},
  {{ $match: {{ ""datosPaciente._id"": {idPatient} }} }},
  {{
    $lookup: {{
      from: ""Alertas"",
      localField: ""IDAlertas"",
      foreignField: ""_id"",
      as: ""datosAlerta""
    }}
  }},
  {{
    $lookup: {{
      from: ""Usuario"",
      localField: ""IDMedico"",
      foreignField: ""_id"",
      as: ""medico""
    }}
  }},
  {{ $unwind: ""$medico"" }},
  {{
    $project: {{
      fecha: 1,
      motivo: 1,
      diagnostico: 1,
      tratamiento: 1,
      observaciones: 1,
      IDMedico: 1,
      IDPaciente: 1,
      datosAlerta: 1,
      nombreCompletoMedico: {{
        $concat: [
          ""$medico.nombre"", "" "",
          ""$medico.apellidoPa"", "" "",
          {{ $ifNull: [""$medico.apellidoMa"", """"] }}
        ]
      }}
    }}
  }}
]
";

        var bsonArray = BsonSerializer.Deserialize<BsonArray>(reports); // VAR
        var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

        var pipeline = PipelineDefinition<Report, ReportAndDetails>.Create(bsonDocuments);// TASK

        var result = await _report.Aggregate(pipeline).ToListAsync();

        return result;
    }

    public async Task Create(Report report)
    {
        var last = await _report.Find(_ => true)
                       .SortByDescending(u => u.Id)
                       .Limit(1).FirstOrDefaultAsync();

        report.Id = last != null ? last.Id + 1 : 1;
        report.Date = DateTime.Today;

        await _report.InsertOneAsync(report);
    }
}

