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

        public async Task<List<Alert>> GetAll() =>
            await _alerts.Find(_ => true).ToListAsync();

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
    }

}