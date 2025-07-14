using MongoDB.Driver;
using SenseCareLocal.Config;
using Microsoft.Extensions.Options;

public class DeviceService
{
    private readonly IMongoCollection<Device> _devices;

    public DeviceService(IOptions<MongoDBSettings> config)
    {
        var client = new MongoClient(config.Value.ConnectionString);
        var database = client.GetDatabase(config.Value.DatabaseName);
        _devices = database.GetCollection<Device>("Dispositivo");
    }

    public async Task CreateDevice(Device device)
    {
        var last = await _devices.Find(_ => true)
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
}

