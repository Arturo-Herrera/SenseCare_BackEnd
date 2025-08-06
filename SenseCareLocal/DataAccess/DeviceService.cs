using MongoDB.Driver;
using SenseCareLocal.Config;
using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;

public class DeviceService
{
    private readonly IMongoCollection<Device> _devices;
    private readonly IMongoCollection<Patient> _patient;

    public DeviceService(IOptions<MongoDBSettings> config)
    {
        var client = new MongoClient(config.Value.ConnectionString);
        var database = client.GetDatabase(config.Value.DatabaseName);
        _devices = database.GetCollection<Device>("Dispositivo");
        _patient = database.GetCollection<Patient>("Paciente");
    }

    public async Task CreateDevice(Device device)
    {
        var last = await _devices.Find( _=> true)
                                 .SortByDescending(d => d.Id)
                                 .Limit(1)
                                 .FirstOrDefaultAsync();

        device.Id = last != null ? last.Id + 1 : 1;
        device.Fecha = DateTime.UtcNow;
        device.Activo = true;

        await _devices.InsertOneAsync(device);
    }

    public async Task<Device?> GetById(int id) =>
    await _devices.Find(d => d.Id == id).FirstOrDefaultAsync();

    public async Task<List<DeviceId>> GetAvailableDevices()
    {
        var devices = @"
    [
        {
            ""$match"": { ""activo"": true }
        },
        {
            ""$lookup"": {
                ""from"": ""Paciente"",
                ""localField"": ""_id"",
                ""foreignField"": ""IDDispositivo"",
                ""as"": ""dispositivoAsignado""
            }
        },
        {
            ""$match"": {
                ""dispositivoAsignado"": { ""$eq"": [] }
            }
        },
        {
            ""$project"": {
                _id: 1
            }
        }
    ]
    ";

        var bsonArray = BsonSerializer.Deserialize<BsonArray>(devices);
        var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

        var pipeline = PipelineDefinition<Device, DeviceId>.Create(bsonDocuments);
        var result = await _devices.Aggregate(pipeline).ToListAsync();

        return result;
    }

    public async Task<bool> AssignDeviceToPatient(int idPaciente, int idDispositivo)
    {
        var patientFilter = Builders<Patient>.Filter.Eq(p => p.IDUsuario, idPaciente);
        var patient = await _patient.Find(patientFilter).FirstOrDefaultAsync();

        if (patient == null)
            return false;

        var deviceFilter = Builders<Device>.Filter.Eq(d => d.Id, idDispositivo);
        var device = await _devices.Find(deviceFilter).FirstOrDefaultAsync();

        if (device == null)
            return false;

        // Actualizar el paciente con el nuevo dispositivo asignado
        var update = Builders<Patient>.Update.Set(p => p.IDDispositivo, idDispositivo);
        await _patient.UpdateOneAsync(patientFilter, update);

        // También puedes actualizar el dispositivo (por ejemplo, marcarlo como asignado)
        var updateDevice = Builders<Device>.Update.Set(d => d.Activo, true);
        await _devices.UpdateOneAsync(deviceFilter, updateDevice);

        return true;
    }

}