
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using SenseCareLocal.Config;

namespace SenseCareLocal.DataAccess
{
    public class MapService
    {
        private readonly IMongoCollection<Device> _devices;

        public MapService(IOptions<MongoDBSettings> config)
        {
            var client = new MongoClient(config.Value.ConnectionString);
            var database = client.GetDatabase(config.Value.DatabaseName);
            _devices = database.GetCollection<Device>("Dispositivo");
        }

        public async Task<List<Map>> GetAllDevicesWithPatient(int idMedic)
        {
            var deviceAndPatient = $@"
                [
                  {{
                    $lookup: {{
                      from: ""Paciente"",
                      localField: ""_id"",
                      foreignField: ""IDDispositivo"",
                      as: ""paciente""
                    }}
                  }},
                  {{ $unwind: ""$paciente"" }},
                  {{
                    $match: {{
                      ""paciente.IDMedico"": {idMedic}
                    }}
                  }},
                  {{
                    $lookup: {{
                      from: ""Usuario"",
                      localField: ""paciente.IDUsuario"",
                      foreignField: ""_id"",
                      as: ""usuario""
                    }}
                  }},
                  {{ $unwind: ""$usuario"" }},
                  {{
                    $lookup: {{
                      from: ""Alertas"",
                      localField: ""paciente._id"",
                      foreignField: ""IDPaciente"",
                      as: ""alertas""
                    }}
                  }},
                  {{ $unwind: ""$alertas"" }},
                  {{ $sort: {{ ""alertas.fecha"": -1 }} }},
                  {{
                    $group: {{
                      _id: ""$paciente._id"",
                      nombreCompleto: {{ $first: {{ $concat: [""$usuario.nombre"", "" "", ""$usuario.apellidoPa"", "" "", {{ $ifNull: [""$usuario.apellidoMa"", """" ] }} ] }} }},
                      sexo: {{ $first: ""$usuario.sexo"" }},
                      latitud: {{ $first: ""$latitud"" }},
                      longitud: {{ $first: ""$longitud"" }},
                      ultimaAlerta: {{ $first: ""$alertas"" }}
                    }}
                  }},
                  {{
                    $project: {{
                      _id: 0,
                      nombreCompleto: 1,
                      sexo: 1,
                      latitud: 1,
                      longitud: 1,
                      alerta: ""$ultimaAlerta""
                    }}
                  }}
                ]
                ";

            var bsonArray = BsonSerializer.Deserialize<BsonArray>(deviceAndPatient);
            var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

            var pipeline = PipelineDefinition<Device, Map>.Create(bsonDocuments);

            var result = await _devices.Aggregate(pipeline).ToListAsync();

            return result;
        }
    }
}
