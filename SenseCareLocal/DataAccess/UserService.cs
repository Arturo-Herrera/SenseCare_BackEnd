using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SenseCareAPI.Helpers;
using SenseCareLocal.Config;

namespace SenseCareLocal.Services
{
    public class UserService
    {
        private readonly IMongoCollection<User> _users;

        public UserService(IOptions<MongoDBSettings> config)
        {
            var client = new MongoClient(config.Value.ConnectionString);
            var database = client.GetDatabase(config.Value.DatabaseName);
            _users = database.GetCollection<User>("Usuario");
        }

        public async Task<List<User>> GetAll() =>
            await _users.Find(_ => true).ToListAsync();

        public async Task Create(User user)
        {
            var last = await _users.Find(_ => true)
                                   .SortByDescending(u => u.Id)
                                   .Limit(1).FirstOrDefaultAsync();
            user.Id = last != null ? last.Id + 1 : 1;
            user.FecNac = user.FecNac.Date;
            user.Contrasena = PasswordHelper.Hash(user.Contrasena);

            await _users.InsertOneAsync(user);

        }

        public async Task<User> GetById(int id) =>

            await _users.Find(u => u.Id == id).FirstOrDefaultAsync();

        public async Task<User?> GetByEmail(string email) =>
    await _users.Find(u => u.Email == email).FirstOrDefaultAsync();


        public async Task<int> CreateAndReturnId(User user)
        {
            var last = await _users.Find(_ => true)
                                   .SortByDescending(u => u.Id)
                                   .Limit(1)
                                   .FirstOrDefaultAsync();

            user.Id = last != null ? last.Id + 1 : 1;
            user.Contrasena = PasswordHelper.Hash(user.Contrasena);
            user.FecNac = user.FecNac.Date;
            user.Activo = true;

            await _users.InsertOneAsync(user);
            return user.Id;
        }
    }
}
