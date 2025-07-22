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

        var bsonArray = BsonSerializer.Deserialize<BsonArray>(averageDaily); // VAR
        var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

        var pipeline = PipelineDefinition<VitalSign, AverageVitalsPerDay>.Create(bsonDocuments);// TASK

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

    public async Task<List<AverageVitalSign>> GetAveragePatient(int idPaciente)
    {
        // Calcular fecha de inicio (últimos 7 días)
        var fechaInicio = DateTime.UtcNow.AddDays(-7).Date;

        var avgByPatient = $@"
    [
        {{
            ""$match"": {{ 
                ""IDPaciente"": {idPaciente},
                ""fecha"": {{ ""$gte"": new Date(""{fechaInicio:yyyy-MM-dd}"") }}
            }}
        }},
        {{
            ""$addFields"": {{
                ""promedioPulsoDoc"": {{ ""$avg"": ""$pulso"" }}
            }}
        }},
        {{
            ""$group"": {{
                ""_id"": {{
                    ""IDPaciente"": ""$IDPaciente"",
                    ""dia"": {{ ""$dateToString"": {{ ""format"": ""%Y-%m-%d"", ""date"": ""$fecha"" }} }}
                }},
                ""promedioPulso"": {{ ""$avg"": ""$promedioPulsoDoc"" }},
                ""promedioTemperatura"": {{ ""$avg"": ""$temperatura"" }},
                ""promedioOxigeno"": {{ ""$avg"": ""$oxigeno"" }}
            }}
        }},
        {{
            ""$project"": {{
                ""_id"": 0,
                ""IDPaciente"": ""$_id.IDPaciente"",
                ""dia"": ""$_id.dia"",
                ""promedioPulso"": {{ ""$round"": [""$promedioPulso"", 2] }},
                ""promedioTemperatura"": {{ ""$round"": [""$promedioTemperatura"", 2] }},
                ""promedioOxigeno"": {{ ""$round"": [""$promedioOxigeno"", 2] }}
            }}
        }}
    ]";

        var bsonArray = BsonSerializer.Deserialize<BsonArray>(avgByPatient);
        var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();
        var pipeline = PipelineDefinition<VitalSign, AverageVitalSign>.Create(bsonDocuments);

        return await _signs.Aggregate(pipeline).ToListAsync();
    }

    public async Task<List<LastLectures>> GetLastLectures(int idPatient)
    {
        var vitalSignsQuery = $@"[
        {{
            ""$match"": {{ ""IDPaciente"": {idPatient} }}
        }},
        {{
            ""$addFields"": {{
                ""pulsoPromedio"": {{ ""$avg"": ""$pulso"" }}
            }}
        }},
        {{
            ""$sort"": {{ ""fecha"": -1 }}
        }},
        {{
            ""$limit"": 3
        }},
        {{
            ""$project"": {{
                ""_id"": 1,
                ""fecha"": {{
                    ""$dateToString"": {{
                        ""format"": ""%Y-%m-%d %H:%M:%S"",
                        ""date"": ""$fecha""
                    }}
                }},
                ""fuente"": {{
                    ""$cond"": {{
                        ""if"": {{ ""$eq"": [""$fuente"", true] }},
                        ""then"": ""Manual"",
                        ""else"": ""Automática""
                    }}
                }},
                ""pulso"": 1,
                ""pulsoPromedio"": {{ ""$round"": [""$pulsoPromedio"", 2] }},
                ""temperatura"": 1,
                ""oxigeno"": 1
            }}
        }}
    ]";

        var bsonArray = BsonSerializer.Deserialize<BsonArray>(vitalSignsQuery); // VAR
        var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

        var pipeline = PipelineDefinition<VitalSign, LastLectures>.Create(bsonDocuments);// TASK

        var result = await _signs.Aggregate(pipeline).ToListAsync();

        return result;
    }
}