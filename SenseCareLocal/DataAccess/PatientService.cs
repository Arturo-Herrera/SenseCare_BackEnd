using Microsoft.Extensions.Options;
using MongoDB.Driver;
using SenseCareAPI.Helpers;
using SenseCareLocal.Config;
using SenseCareLocal.Services;

public class PatientService
{
    private readonly IMongoCollection<Patient> _patients;
    private readonly UserService _userService;

    public PatientService(IOptions<MongoDBSettings> config, UserService userService)
    {
        var client = new MongoClient(config.Value.ConnectionString);
        var database = client.GetDatabase(config.Value.DatabaseName);
        _patients = database.GetCollection<Patient>("Paciente");
        _userService = userService;
    }

    public async Task<bool> RegisterPatient(RegisterPatientDto dto)
    {
        // Crear usuario y obtener ID
        var user = new User
        {
            Nombre = dto.Nombre,
            ApellidoPa = dto.ApellidoPa,
            ApellidoMa = dto.ApellidoMa,
            FecNac = dto.FecNac.Date,
            Sexo = dto.Sexo,
            DirColonia = dto.DirColonia,
            DirCalle = dto.DirCalle,
            DirNum = dto.DirNum,
            Telefono = dto.Telefono,
            Email = dto.Email,
            Contrasena = PasswordHelper.Hash(dto.Contrasena),
            Activo = true,
            IDTipoUsuario = new UserRole { Id = "PAC", Descripcion = "Paciente" }
        };

        var idUser = await _userService.CreateAndReturnId(user);

        if (idUser == 0) return false;

        // Conseguir último ID de paciente
        var last = await _patients.Find(_ => true).SortByDescending(p => p.Id).Limit(1).FirstOrDefaultAsync();
        var newId = last != null ? last.Id + 1 : 1;

        var patient = new Patient
        {
            Id = newId,
            IDUsuario = idUser,
            IDCuidador = dto.IDCuidador,
            IDMedico = dto.IDMedico,
            IDDispositivo = dto.IDDispositivo
        };

        await _patients.InsertOneAsync(patient);
        return true;
    }
    public async Task<Patient?> ObtenerPacientePorCuidadorAsync(int cuidadorId)
    {
        return await _patients.Find(p => p.IDCuidador == cuidadorId).FirstOrDefaultAsync();
    }
}