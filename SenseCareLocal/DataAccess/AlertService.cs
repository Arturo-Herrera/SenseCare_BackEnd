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

        public async Task<List<AlertsPerDay>> GetTotalsToday()
        {
            var currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var alerts = $@"
    [
      {{
        '$match': {{
          '$expr': {{
            '$eq': [
              {{ '$dateToString': {{ 'format': '%Y-%m-%d', 'date': '$fecha' }} }},
              '{currentDate}'
            ]
          }}
        }}
      }},
      {{
        '$group': {{
          '_id': null,
          'datos': {{
            '$push': {{
              'fecha': {{
                '$dateToString': {{ 'format': '%Y-%m-%d', 'date': '$fecha' }}
              }}
            }}
          }}
        }}
      }},
      {{ '$unwind': '$datos' }},
      {{
        '$group': {{
          '_id': '$datos.fecha',
          'totalAlertas': {{ '$sum': 1 }}
        }}
      }},
      {{
        '$project': {{
          '_id': 0,
          'fecha': '$_id',
          'totalAlertas': 1
        }}
      }},
      {{ '$sort': {{ 'fecha': 1 }} }}
    ]";

            var bsonArray = BsonSerializer.Deserialize<BsonArray>(alerts);
            var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

            var pipeline = PipelineDefinition<Alert, AlertsPerDay>.Create(bsonDocuments);

            var result = await _alerts.Aggregate(pipeline).ToListAsync();

            return result;


        }

    }
}
