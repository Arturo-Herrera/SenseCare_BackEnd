using Microsoft.Extensions.Options;
using MongoDB.Bson.Serialization;
using MongoDB.Bson;
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

        public async Task<List<UserFilterData>> UserFilter(UserFilter filtro)
        {
            var matchStage = UsersMatchBuilder.MatchBuilder(filtro);

            var pipelineString = "[";

            if (!string.IsNullOrEmpty(matchStage))
                pipelineString += matchStage + ",";

            pipelineString += @"
            {
                ""$project"": {
                    ""_id"": 1,
                    ""nombreCompleto"": {
                        ""$concat"" : [""$nombre"", "" "", ""$apellidoPa"", "" "", {""$ifNull"": [""$apellidoMa"", """"]}]
                    },
                    ""fecNac"": 1,
                    ""sexo"": 1,
                    ""direccionCompleta"" : {
                        ""$concat"" : [""$dirColonia"", "" "", ""$dirCalle"", "" "", ""$dirNum""]
                    },
                    ""telefono"": 1,
                    ""email"": 1,
                    ""activo"": 1,
                    ""IDTipoUsuario"": ""$IDTipoUsuario._id"",
                    ""DescripcionTipoUsuario"": ""$IDTipoUsuario.descripcion""
                }
            }
            ]";

            var bsonArray = BsonSerializer.Deserialize<BsonArray>(pipelineString);
            var pipeline = PipelineDefinition<User, UserFilterData>.Create(
                bsonArray.Select(x => x.AsBsonDocument)
            );

            return await _users.Aggregate(pipeline).ToListAsync();
        }

        public async Task<List<CaregiverId>> GetCaregivers()
        {
            var caregivers = @"
            [
                {
                    ""$match"": {
                        ""IDTipoUsuario._id"": ""CUID"",
                        ""activo"": true
                    }
                },
                {
                    ""$project"": {
                        _id: 1
                    }
                }
            ]
        ";


            var bsonArray = BsonSerializer.Deserialize<BsonArray>(caregivers);
            var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

            var pipeline = PipelineDefinition<User, CaregiverId>.Create(bsonDocuments);
            var result = await _users.Aggregate(pipeline).ToListAsync();

            return result;
        }

        public async Task<Usuario> DisableUser(int idUser)
        {
            var filter = $@"{{ _id: {idUser} }}";
            var update = $@"{{ $set: {{ activo: false }} }}";

            var filterDoc = BsonSerializer.Deserialize<BsonDocument>(filter);
            var updateDoc = BsonSerializer.Deserialize<BsonDocument>(update);

            var options = new FindOneAndUpdateOptions<User, Usuario>
            {
                ReturnDocument = ReturnDocument.After
            };

            var result = await _users.FindOneAndUpdateAsync(filterDoc, updateDoc, options);
            return result;
        }

        public async Task<Usuario> EnableUser(int idUser)
        {
            var filter = $@"{{ _id: {idUser} }}";
            var update = $@"{{ $set: {{ activo: true }} }}";

            var filterDoc = BsonSerializer.Deserialize<BsonDocument>(filter);
            var updateDoc = BsonSerializer.Deserialize<BsonDocument>(update);

            var options = new FindOneAndUpdateOptions<User, Usuario>
            {
                ReturnDocument = ReturnDocument.After
            };

            var result = await _users.FindOneAndUpdateAsync(filterDoc, updateDoc, options);
            return result;
        }

    }
}
