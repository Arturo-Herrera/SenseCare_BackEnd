using Microsoft.Extensions.Options;
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

        public async Task<List<Alert>> GetAll() =>
            await _alerts.Find(_ => true).ToListAsync();

    }
}
