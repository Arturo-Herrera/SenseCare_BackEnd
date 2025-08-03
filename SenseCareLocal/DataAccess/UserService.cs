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

        public async Task<User> Create(User user)
        {
            var last = await _users.Find(_ => true)
                                   .SortByDescending(u => u.Id)
                                   .Limit(1).FirstOrDefaultAsync();
            user.Id = last != null ? last.Id + 1 : 1;
            user.FecNac = user.FecNac.Date;
            user.Contrasena = PasswordHelper.Hash(user.Contrasena);
            await _users.InsertOneAsync(user);

            return user; // Devolver el usuario con el ID asignado
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
            user.Contrasena = "";
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
                    ""nombre"": 1,
                    ""apellidoPa"": 1,
                    ""apellidoMa"": 1,
                    ""fecNac"": 1,
                    ""sexo"": 1,
                    ""dirColonia"": 1,
                    ""dirCalle"": 1,
                    ""dirNum"": 1,
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

        public async Task<List<CaregiverId>> GetAvailableCaregivers()
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
            ""$lookup"": {
                ""from"": ""Paciente"",
                ""localField"": ""_id"",
                ""foreignField"": ""IDCuidador"",
                ""as"": ""pacienteAsignado""
            }
        },
        {
            ""$match"": {
                ""pacienteAsignado"": { ""$eq"": [] }
            }
        },
        {
            ""$project"": {
                _id: 1,
                nombreCompleto: {
                    ""$concat"": [""$nombre"", "" "", ""$apellidoPa"", "" "", ""$apellidoMa""]
                }
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

        public async Task<Usuario> UpdateUser(Usuario user)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, user.Id);

            var update = Builders<User>.Update
                .Set(u => u.Nombre, user.Nombre)
                .Set(u => u.ApellidoPa, user.ApellidoPa)
                .Set(u => u.ApellidoMa, user.ApellidoMa ?? "")
                .Set(u => u.FecNac, user.FecNac.Date)
                .Set(u => u.Sexo, user.Sexo)
                .Set(u => u.DirColonia, user.DirColonia)
                .Set(u => u.DirCalle, user.DirCalle)
                .Set(u => u.DirNum, user.DirNum)
                .Set(u => u.Telefono, user.Telefono)
                .Set(u => u.Email, user.Email)
                .Set(u => u.Activo, user.Activo);

            if (!string.IsNullOrWhiteSpace(user.Contrasena))
            {
                update = update.Set(u => u.Contrasena, PasswordHelper.Hash(user.Contrasena));
            }

            if (user.IDTipoUsuario != null)
            {
                update = update.Set(u => u.IDTipoUsuario, user.IDTipoUsuario);
            }

            var options = new FindOneAndUpdateOptions<User>
            {
                ReturnDocument = ReturnDocument.After
            };

            var updatedUser = await _users.FindOneAndUpdateAsync(filter, update, options);

            return new Usuario
            {
                Id = updatedUser.Id,
                Nombre = updatedUser.Nombre,
                ApellidoPa = updatedUser.ApellidoPa,
                ApellidoMa = updatedUser.ApellidoMa,
                FecNac = updatedUser.FecNac,
                Sexo = updatedUser.Sexo,
                DirColonia = updatedUser.DirColonia,
                DirCalle = updatedUser.DirCalle,
                DirNum = updatedUser.DirNum,
                Telefono = updatedUser.Telefono,
                Email = updatedUser.Email,
                Contrasena = updatedUser.Contrasena,
                Activo = updatedUser.Activo,
                IDTipoUsuario = updatedUser.IDTipoUsuario
            };
        }


    }
}
