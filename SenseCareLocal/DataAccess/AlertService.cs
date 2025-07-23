using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using SenseCareLocal.Config;

namespace SenseCareLocal.Services
{
    public class AlertService
    {
        private readonly IMongoCollection<Alert> _alerts;

        public AlertService(IOptions<MongoDBSettings> config)
        {
            var client = new MongoClient(config.Value.ConnectionString);
            var database = client.GetDatabase(config.Value.DatabaseName);
            _alerts = database.GetCollection<Alert>("Alertas");
        }

        public async Task<List<Alert>> GetAll(int idPatient)
        {
            var All = $@"
            [
              {{ 
                $match:{{
                  IDPaciente : {idPatient}
                }}
              }},
              {{
                $project:{{
                  _id: 1,
                  fecha: 1,
                  signoAfectado: 1,
                  IDPaciente: 1,
                  IDTipoAlerta: 1
                }}
              }}
            ]";

            var bsonArray = BsonSerializer.Deserialize<BsonArray>(All); // VAR
            var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

            var pipeline = PipelineDefinition<Alert, Alert>.Create(bsonDocuments);// TASK

            var result = await _alerts.Aggregate(pipeline).ToListAsync();

            return result;
        }

        public async Task Create(Alert alert)
        {
            var last = await _alerts.Find(_ => true)
                                       .SortByDescending(u => u.Id)
                                       .Limit(1).FirstOrDefaultAsync();
            alert.Id = last != null ? last.Id + 1 : 1;

            //falta fecha, signoafectado, idpaciente, idtipoalerta
            alert.Fecha = DateTime.Now;
            alert.SignoAfectado = alert.SignoAfectado ?? "N/A"; // Default value if null
            alert.IDPaciente = alert.IDPaciente > 0 ? alert.IDPaciente : 1; // Default value if not set
            alert.IDTipoAlerta = alert.IDTipoAlerta ?? "N/A"; // Default value if null
        }//CAMBIAR SI SON NULL Y DATOS DE RETORNO

        public async Task<AlertsPerDay> GetTotalsToday()
        {
            var alerts = $@"
                [
                    {{
                      '$group': {{
                        '_id': {{ '$dateToString': {{ 'format': '%Y-%m-%d', 'date': '$fecha' }} }},
                        'totalAlertas': {{ '$sum': 1 }}
                      }}
                    }},
                    {{
                      '$group': {{
                        '_id': null,
                        'promedio': {{ '$avg': '$totalAlertas' }}
                      }}
                    }},
                    {{
                      '$project': {{
                        '_id': 0,
                        'promedioAlertasPorDia': {{ '$round': [ '$promedio', 0 ] }}
                      }}
                    }}
                ]";

            var bsonArray = BsonSerializer.Deserialize<BsonArray>(alerts);
            var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

            var pipeline = PipelineDefinition<Alert, AlertsPerDay>.Create(bsonDocuments);

            var result = await _alerts.Aggregate(pipeline).FirstOrDefaultAsync();

            return result;

        }

        public async Task<List<LastAlerts>> GetLastAlerts(int idPatient) // MINUTOS DESDE QUE PASARON
        {
            var lastAlerts = $@"
                [
                    {{ ""$match"": {{ ""IDPaciente"": {idPatient} }} }},
                    {{ ""$sort"": {{ ""fecha"": -1 }} }},
                    {{ ""$limit"": 6 }},
                    {{
                    ""$lookup"": {{
                        ""from"": ""TipoAlerta"",
                        ""localField"": ""IDTipoAlerta"",
                        ""foreignField"": ""_id"",
                        ""as"": ""tipoAlerta""
                    }}
                    }},
                    {{ ""$unwind"": ""$tipoAlerta"" }},
                    {{
                    ""$lookup"": {{
                        ""from"": ""Paciente"",
                        ""localField"": ""IDPaciente"",
                        ""foreignField"": ""_id"",
                        ""as"": ""datosPaciente""
                    }}
                    }},
                    {{ ""$unwind"": ""$datosPaciente"" }},
                    {{
                    ""$lookup"": {{
                        ""from"": ""Usuario"",
                        ""localField"": ""datosPaciente.IDUsuario"",
                        ""foreignField"": ""_id"",
                        ""as"": ""datosUsuario""
                    }}
                    }},
                    {{ ""$unwind"": ""$datosUsuario"" }},
                    {{
                    ""$addFields"": {{
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
                    ""$project"": {{
                        ""_id"": 1,
                        ""fecha"": {{
                        ""$dateToString"": {{
                            ""format"": ""%Y-%m-%d %H:%M:%S"",
                            ""date"": ""$fecha""
                        }}
                        }},
                        ""signoAfectado"": 1,
                        ""tiempoTranscurrido"": 1,
                        ""tipoAlerta"": {{
                        ""tipo"": ""$tipoAlerta.tipo"",
                        ""descripcion"": ""$tipoAlerta.descripcion"",
                        ""prioridad"": ""$tipoAlerta.prioridad""
                        }},
                        ""paciente"": {{
                        ""_id"": ""$datosPaciente._id"",
                        ""nombre"": ""$datosUsuario.nombre""
                        }}
                    }}
                    }}
                ]";

            var bsonArray = BsonSerializer.Deserialize<BsonArray>(lastAlerts);
            var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

            var pipeline = PipelineDefinition<Alert, LastAlerts>.Create(bsonDocuments);
            var result = await _alerts.Aggregate(pipeline).ToListAsync();

            return result;
        }

    }

}