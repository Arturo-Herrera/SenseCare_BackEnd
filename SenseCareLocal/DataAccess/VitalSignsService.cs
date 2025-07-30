using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using SenseCareLocal.Config;

public class VitalSignsService
{
    private readonly IMongoCollection<VitalSign> _signs;
    private readonly IMongoCollection<Patient> _patient;

    public VitalSignsService(IOptions<MongoDBSettings> cfg)
    {
        var client = new MongoClient(cfg.Value.ConnectionString);
        var db = client.GetDatabase(cfg.Value.DatabaseName);
        _signs = db.GetCollection<VitalSign>("SignosVitales");
        _patient = db.GetCollection<Patient>("Paciente");
    }

    public async Task<List<VitalSign>> GetByPatient(int idPaciente)
    {
        return await _signs.Find(s => s.IDPaciente == idPaciente)
                            .SortByDescending(s => s.Fecha)
                            .Limit(1)
                            .ToListAsync();
    }

    public async Task InsertPulse(int idDevice, double value)
    {
        var patient = await _patient.Find(p => p.IDDispositivo == idDevice).FirstOrDefaultAsync();
        if (patient == null)
            throw new Exception("Patient not found");

        // Filtro para buscar documentos del paciente donde falta temperatura o oxigeno
        var filtro = Builders<VitalSign>.Filter.And(
            Builders<VitalSign>.Filter.Eq(s => s.IDPaciente, patient.Id),
            Builders<VitalSign>.Filter.Or(
                Builders<VitalSign>.Filter.Eq(s => s.Temperatura, 0),
                Builders<VitalSign>.Filter.Eq(s => s.Oxigeno, 0)
            //Builders<VitalSign>.Filter.Eq(s => s.Temperatura, BsonNull.Value),
            //Builders<VitalSign>.Filter.Eq(s => s.Oxigeno, BsonNull.Value)
            )
        );

        var last = await _signs
            .Find(filtro)
            .SortByDescending(s => s.Fecha)
            .FirstOrDefaultAsync();

        if (last != null)
        {
            var update = Builders<VitalSign>.Update.Push(s => s.Pulso, value);
            await _signs.UpdateOneAsync(s => s.Id == last.Id, update);
        }
        else
        {
            // Obtener el máximo ID personalizado y sumarle 1
            var maxIdDoc = await _signs
                    .Find(Builders<VitalSign>.Filter.Empty)
                    .SortByDescending(s => s.Id)
                    .Limit(1)
                    .FirstOrDefaultAsync();

            int nextId = (maxIdDoc?.Id ?? 0) + 1;

            var newDoc = new VitalSign
            {
                Id = nextId,
                IDPaciente = patient.Id,
                Fecha = DateTime.Now,
                Pulso = new List<double> { value },
                Temperatura = 0,
                Oxigeno = 0,
                Fuente = false // o true si aplica
            };
            await _signs.InsertOneAsync(newDoc);
        }
    }




    public async Task InsertTemperatureAndOxygen(int idDevice, double? temperature, double? oxygen)
    {
        var patient = await _patient.Find(p => p.IDDispositivo == idDevice).FirstOrDefaultAsync();
        if (patient == null)
            throw new Exception("Patient not found");

        var filter = Builders<VitalSign>.Filter.Eq(s => s.IDPaciente, patient.Id);
        var last = await _signs
            .Find(filter)
            .SortByDescending(s => s.Fecha)
            .FirstOrDefaultAsync();

        bool needsNew = true;

        if (last != null)
        {
            var updateDef = Builders<VitalSign>.Update.Combine();

            if (temperature.HasValue && (last.Temperatura == 0 || last.Temperatura == null))
            {
                updateDef = updateDef.Set(s => s.Temperatura, temperature.Value);
                needsNew = false;
            }

            if (oxygen.HasValue && (last.Oxigeno == 0 || last.Oxigeno == null))
            {
                updateDef = updateDef.Set(s => s.Oxigeno, oxygen.Value);
                needsNew = false;
            }

            if (!needsNew)
            {
                await _signs.UpdateOneAsync(s => s.Id == last.Id, updateDef);
                return;
            }
        }

        // Buscar el max Id para generar uno nuevo único
        var maxIdDoc = await _signs.Find(FilterDefinition<VitalSign>.Empty)
                                  .SortByDescending(s => s.Id)
                                  .Limit(1)
                                  .FirstOrDefaultAsync();

        int nextId = (maxIdDoc != null) ? maxIdDoc.Id + 1 : 1;

        var newDoc = new VitalSign
        {
            Id = nextId,
            IDPaciente = patient.Id,
            Fecha = DateTime.Now,
            Pulso = new List<double>(),
            Temperatura = temperature,
            Oxigeno = oxygen
        };

        await _signs.InsertOneAsync(newDoc);
    }



    public async Task<List<AverageVitalsPerDay>> GetAverageVitalsPerDay(int idMedic)
    {
        var fechaInicio = DateTime.UtcNow.AddDays(-7).Date;

        var averageDaily = $@"
[
  {{
    ""$match"": {{
      ""fecha"": {{ ""$gte"": ISODate(""{fechaInicio:yyyy-MM-dd}T00:00:00.000Z"") }}
    }}
  }},
  {{
    ""$lookup"": {{
      ""from"": ""Paciente"",
      ""localField"": ""IDPaciente"",
      ""foreignField"": ""_id"",
      ""as"": ""paciente""
    }}
  }},
  {{
    ""$unwind"": ""$paciente""
  }},
  {{
    ""$match"": {{
      ""paciente.IDMedico"": {idMedic}
    }}
  }},
  {{
    ""$group"": {{
      ""_id"": null,
      ""datos"": {{
        ""$push"": {{
          ""fecha"": {{
            ""$dateToString"": {{ ""format"": ""%Y-%m-%d"", ""date"": ""$fecha"" }}
          }},
          ""temperatura"": ""$temperatura"",
          ""pulso"": ""$pulso""
        }}
      }}
    }}
  }},
  {{
    ""$unwind"": ""$datos""
  }},
  {{
    ""$group"": {{
      ""_id"": ""$datos.fecha"",
      ""promedioTemperatura"": {{ ""$avg"": ""$datos.temperatura"" }},
      ""promedioPulso"": {{ ""$avg"": {{ ""$avg"": ""$datos.pulso"" }} }}
    }}
  }},
  {{
    ""$project"": {{
      ""_id"": 0,
      ""fecha"": ""$_id"",
      ""promedioTemperatura"": {{ ""$round"": [""$promedioTemperatura"", 2] }},
      ""promedioPulso"": {{ ""$round"": [""$promedioPulso"", 2] }}
    }}
  }},
  {{
    ""$sort"": {{ ""fecha"": 1 }}
  }}
]
";

        var bsonArray = BsonSerializer.Deserialize<BsonArray>(averageDaily);
        var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();
        var pipeline = PipelineDefinition<VitalSign, AverageVitalsPerDay>.Create(bsonDocuments);

        var result = await _signs.Aggregate(pipeline).ToListAsync();

        return result;
    }


    public async Task<AverageOxygenLevel> GetOxygenLevelAvg(int idMedic)
    {
        var avgOxygen = $@"
    [
        {{
            ""$lookup"": {{
                ""from"": ""Paciente"",
                ""localField"": ""IDPaciente"",
                ""foreignField"": ""_id"",
                ""as"": ""paciente""
            }}
        }},
        {{
            ""$unwind"": ""$paciente""
        }},
        {{
            ""$match"": {{
                ""paciente.IDMedico"": {idMedic}
            }}
        }},
        {{
            ""$group"": {{
                ""_id"": null,
                ""promedioOxigeno"": {{ ""$avg"": ""$oxigeno"" }}
            }}
        }},
        {{
            ""$project"": {{
                ""_id"": 0,
                ""promedioOxigeno"": {{ ""$round"": [""$promedioOxigeno"", 2] }}
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
                ""dia"": {{
                  ""$dateTrunc"": {{
                    ""date"": ""$fecha"",
                    ""unit"": ""day""
                  }}
                }}
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

    public async Task<List<LastLectures>> GetLastLectures(int idPatient) // MINUTOS DESDE QUE PASARON
    {
        var vitalSignsQuery = $@"[
    {{
        ""$match"": {{ ""IDPaciente"": {idPatient} }}
    }},
    {{
        ""$addFields"": {{
            ""pulsoPromedio"": {{ ""$avg"": ""$pulso"" }},
            ""tiempoTranscurrido"": {{
                        ""$dateDiff"": {{
                            ""startDate"": ""$fecha"",
                            ""endDate"": ""$$NOW"",
                            ""unit"": ""minute""
                        }}
            }}
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
            ""tiempoTranscurrido"": 1,
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