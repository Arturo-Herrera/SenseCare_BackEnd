using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SenseCareLocal.Config;

public class AlertService
{
    private readonly IMongoCollection<Alert> _alerts;

    public AlertService(IOptions<MongoDBSettings> config)
    {
        var client = new MongoClient(config.Value.ConnectionString);
        var database = client.GetDatabase(config.Value.DatabaseName);
        _alerts = database.GetCollection<Alert>("Alertas");
    }

    public async Task<List<Alert>> GetAll() =>
        await _alerts.Find(_ => true).ToListAsync();

    public async Task<List<Alert>> GetByPatient(int patientId) =>
        await _alerts.Find(alert => alert.IDPaciente == patientId)
                     .SortByDescending(alert => alert.Fecha)
                     .ToListAsync();

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

}