using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SenseCareLocal.Config;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SenseCareLocal.Services
{
    public class InformeService
    {
        private readonly IMongoCollection<Informe> _informes;

        public InformeService(IOptions<MongoDBSettings> config)
        {
            var client = new MongoClient(config.Value.ConnectionString);
            var database = client.GetDatabase(config.Value.DatabaseName);
            _informes = database.GetCollection<Informe>("Informe");
        }

        public async Task<List<Informe>> GetAll() =>
            await _informes.Find(_ => true).ToListAsync();

        public async Task<Informe?> GetById(int id) =>
            await _informes.Find(i => i.Id == id).FirstOrDefaultAsync();

        public async Task Create(Informe informe)
        {
            var last = await _informes.Find(_ => true)
                                      .SortByDescending(i => i.Id)
                                      .Limit(1)
                                      .FirstOrDefaultAsync();

            informe.Id = last != null ? last.Id + 1 : 1;
            informe.Fecha = informe.Fecha.Date;

            await _informes.InsertOneAsync(informe);
        }

        public async Task<int> CreateAndReturnId(Informe informe)
        {
            var last = await _informes.Find(_ => true)
                                      .SortByDescending(i => i.Id)
                                      .Limit(1)
                                      .FirstOrDefaultAsync();

            informe.Id = last != null ? last.Id + 1 : 1;
            informe.Fecha = informe.Fecha.Date;

            await _informes.InsertOneAsync(informe);
            return informe.Id;
        }
    }
}
