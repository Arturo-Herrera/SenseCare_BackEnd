using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
using MongoDB.Driver;
using SenseCareAPI.Helpers;
using SenseCareLocal.Config;
using SenseCareLocal.Services;
public class PatientService
{
    private readonly IMongoCollection<Patient> _patients;
    private readonly UserService _userService;

    public PatientService(IOptions<MongoDBSettings> config, UserService userService)
    {
        var client = new MongoClient(config.Value.ConnectionString);
        var database = client.GetDatabase(config.Value.DatabaseName);
        _patients = database.GetCollection<Patient>("Paciente");
        _userService = userService;
    }

    public async Task<bool> RegisterPatient(RegisterPatientDto dto)
    {
        // Crear usuario y obtener ID
        var user = new User
        {
            Nombre = dto.Nombre,
            ApellidoPa = dto.ApellidoPa,
            ApellidoMa = dto.ApellidoMa,
            Foto = dto.Foto,
            FecNac = dto.FecNac.Date,
            Sexo = dto.Sexo,
            DirColonia = dto.DirColonia,
            DirCalle = dto.DirCalle,
            DirNum = dto.DirNum,
            Telefono = dto.Telefono,
            Email = "",
            Contrasena = "",
            Activo = true,
            IDTipoUsuario = new UserRole { Id = "PAC", Descripcion = "Patient" }
        };

        var idUser = await _userService.CreateAndReturnId(user);

        if (idUser == 0) return false;

        // Conseguir último ID de paciente
        var last = await _patients.Find(_ => true).SortByDescending(p => p.Id).Limit(1).FirstOrDefaultAsync();
        var newId = last != null ? last.Id + 1 : 1;

        var patient = new Patient
        {
            Id = newId,
            IDUsuario = idUser,
            IDCuidador = dto.IDCuidador,
            IDMedico = dto.IDMedico,
            IDDispositivo = dto.IDDispositivo
        };

        await _patients.InsertOneAsync(patient);
        return true;
    }
    public async Task<Patient?> ObtenerPacientePorCuidadorAsync(int cuidadorId)
    {
        return await _patients.Find(p => p.IDCuidador == cuidadorId).FirstOrDefaultAsync();
    }

    public async Task<ActivePatients> GetActivePatients(int medicoId)
    {
        var activePatients = $@"
   [
       {{
           ""$lookup"": {{
               ""from"": ""Usuario"",
               ""localField"": ""IDUsuario"",
               ""foreignField"": ""_id"",
               ""as"": ""usuario""
           }}
       }},
       {{
           ""$unwind"": ""$usuario""
       }},
       {{
           ""$match"": {{
               ""usuario.activo"": true,
               ""IDMedico"": {medicoId}
           }}
       }},
       {{
           ""$count"": ""pacientesActivos""
       }}
   ]
   ";


        var bsonArray = BsonSerializer.Deserialize<BsonArray>(activePatients);
        var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

        var pipeline = PipelineDefinition<Patient, ActivePatients>.Create(bsonDocuments);

        var result = await _patients.Aggregate(pipeline).FirstOrDefaultAsync();

        return result;
    }



    public async Task<List<InfoPatientResult>> GetInfoPatient(int idPatient)
    {

        var infoPatient = $@"[
                {{
                ""$match"": {{ ""_id"": {idPatient} }}
                }},
                {{
                ""$lookup"": {{
                    ""from"": ""Usuario"",
                    ""localField"": ""IDUsuario"",
                    ""foreignField"": ""_id"",
                    ""as"": ""datosUsuario""
                }}
                }},
                {{
                ""$lookup"": {{
                    ""from"": ""Usuario"",
                    ""localField"": ""IDCuidador"",
                    ""foreignField"": ""_id"",
                    ""as"": ""datosCuidador""
                }}
                }},
                {{
                ""$lookup"": {{
                    ""from"": ""Dispositivo"",
                    ""localField"": ""IDDispositivo"",
                    ""foreignField"": ""_id"",
                    ""as"": ""datosDispositivo""
                }}
                }},
                {{
                ""$project"": {{
                    ""_id"": 1,
                    ""paciente"": {{
                    ""$arrayElemAt"": [""$datosUsuario"", 0]
                    }},
                    ""cuidador"": {{
                    ""_id"": {{ ""$arrayElemAt"": [""$datosCuidador._id"", 0] }},
                    ""nombre"": {{ ""$arrayElemAt"": [""$datosCuidador.nombre"", 0] }},
                    ""apellidoPa"": {{ ""$arrayElemAt"": [""$datosCuidador.apellidoPa"", 0] }},
                    ""apellidoMa"": {{ ""$arrayElemAt"": [""$datosCuidador.apellidoMa"", 0] }},
                    ""telefono"": {{ ""$arrayElemAt"": [""$datosCuidador.telefono"", 0] }},
                    ""email"": {{ ""$arrayElemAt"": [""$datosCuidador.email"", 0] }},
                    ""sexo"": {{ ""$arrayElemAt"": [""$datosCuidador.sexo"", 0] }},
                    ""fecNac"": {{ ""$arrayElemAt"": [""$datosCuidador.fecNac"", 0] }}
                    }},
                    ""dispositivo"": {{
                    ""$arrayElemAt"": [""$datosDispositivo"", 0]
                    }}
                }}
                }}
            ]";


        var bsonArray = BsonSerializer.Deserialize<BsonArray>(infoPatient); // VAR
        var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

        var pipeline = PipelineDefinition<Patient, InfoPatientResult>.Create(bsonDocuments);// TASK

        var result = await _patients.Aggregate(pipeline).ToListAsync();

        return result;
    }

    public async Task<List<InfoPatientSelect>> GetPatientSelect(int idDoctor)
    {
        var infoPatient = $@"
    [
        {{
            ""$match"": {{
                ""IDMedico"": {idDoctor}
            }}
        }},
        {{
            ""$lookup"": {{
                ""from"": ""Usuario"",
                ""localField"": ""IDUsuario"",
                ""foreignField"": ""_id"",
                ""as"": ""datosUsuario""
            }}
        }},
        {{
            ""$unwind"": ""$datosUsuario""
        }},
        {{
            ""$match"": {{
                ""datosUsuario.activo"": true
            }}
        }},
        {{
            ""$project"": {{
                ""_id"": 1,
                ""nombrePaciente"": {{
                    ""$concat"": [
                        ""$datosUsuario.nombre"", "" "",
                        ""$datosUsuario.apellidoPa"", "" "",
                        {{ ""$ifNull"": [""$datosUsuario.apellidoMa"", """" ] }}
                    ]
                }}
            }}
        }}
    ]";

        var bsonArray = BsonSerializer.Deserialize<BsonArray>(infoPatient);
        var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

        var pipeline = PipelineDefinition<Patient, InfoPatientSelect>.Create(bsonDocuments);

        var result = await _patients.Aggregate(pipeline).ToListAsync();

        return result;
    }

    public async Task<Patient> GetPatientById(int idPaciente)
    {
        var filter = Builders<Patient>.Filter.Eq(p => p.Id, idPaciente);
        return await _patients.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<int?> GetDeviceIdByCuidadorAsync(int idCuidador)
    {
        var filter = Builders<Patient>.Filter.Eq(p => p.IDCuidador, idCuidador);
        var paciente = await _patients.Find(filter).FirstOrDefaultAsync();

        return paciente?.IDDispositivo;
    }

}