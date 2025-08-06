using Microsoft.Extensions.Options;
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
}