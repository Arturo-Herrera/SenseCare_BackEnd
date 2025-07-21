using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using SenseCareLocal.Config;

public class VitalSignsService
{
    private readonly IMongoCollection<VitalSign> _signs;

    public VitalSignsService(IOptions<MongoDBSettings> cfg)
    {
        var client = new MongoClient(cfg.Value.ConnectionString);
        var db = client.GetDatabase(cfg.Value.DatabaseName);
        _signs = db.GetCollection<VitalSign>("SignosVitales");
    }

    public async Task<List<VitalSign>> GetByPatient(int idPaciente)
    {
        return await _signs.Find(s => s.IDPaciente == idPaciente)
                            .SortByDescending(s => s.Fecha)
                            .Limit(1)
                            .ToListAsync();
    }

    public async Task<List<AverageVitalsPerDay>> GetAverageVitalsPerDay()
    {
        var averageDaily = @"
            [
              {
                $group: {
                  _id: null,
                  datos: {
                    $push: {
                      fecha: {
                        $dateToString: { format: ""%Y-%m-%d"", date: ""$fecha"" }
                      },
                      temperatura: ""$temperatura"",
                      pulso: ""$pulso""
                    }
                  }
                }
              },
              {
                $unwind: ""$datos""
              },
              {
                $group: {
                  _id: ""$datos.fecha"",
                  promedioTemperatura: { $avg: ""$datos.temperatura"" },
                  promedioPulso: { $avg: { $avg: ""$datos.pulso"" } }
                }
              },
              {
                $project: {
                  _id: 0,
                  fecha: ""$_id"",
                  promedioTemperatura: { $round: [""$promedioTemperatura"", 2] },
                  promedioPulso: { $round: [""$promedioPulso"", 2] }
                }
              },
              {
                $sort: { fecha: 1 }
              }
            ]
            ";

        var bsonArray = BsonSerializer.Deserialize<BsonArray>(averageDaily);
        var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

        var pipeline = PipelineDefinition<VitalSign, AverageVitalsPerDay>.Create(bsonDocuments);

        var result = await _signs.Aggregate(pipeline).ToListAsync();

        return result;
    }

    public async Task<AverageOxygenLevel> GetOxygenLevelAvg()
    {
        var avgOxygen = $@"
            [
              {{
                $group: {{
                  _id: null, 
                  promedioOxigeno: {{ $avg: ""$oxigeno"" }}
                }}
              }},
              {{
                $project: {{
                  _id: 0,
                  promedioOxigeno: {{ $round: [""$promedioOxigeno"", 0] }}
                }}
              }}
            ]";

        var bsonArray = BsonSerializer.Deserialize<BsonArray>(avgOxygen);
        var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

        var pipeline = PipelineDefinition<VitalSign, AverageOxygenLevel>.Create(bsonDocuments);

        var result = await _signs.Aggregate(pipeline).FirstOrDefaultAsync();

        return result;
    }
}