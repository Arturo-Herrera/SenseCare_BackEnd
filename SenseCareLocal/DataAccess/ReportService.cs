using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using SenseCareLocal.Config;

public class ReportService
{
    private readonly IMongoCollection<Report> _report;

    public ReportService(IOptions<MongoDBSettings> config)
    {
        var client = new MongoClient(config.Value.ConnectionString);
        var database = client.GetDatabase(config.Value.DatabaseName);
        _report = database.GetCollection<Report>("Informe");
    }

    public async Task<List<Report>> GetAllByPatient(int idPatient)
    {
		var reports = $@"
            [
				{{""$lookup"": {{
					""from"": ""Paciente"",
					""localField"": ""IDPaciente"",
					""foreignField"": ""_id"",
					""as"": ""datosPaciente""
				}}}},
				{{ ""$unwind"": ""$datosPaciente""}},
				{{ ""$match"": {{""datosPaciente._id"": {idPatient} }}}},
				{{ ""$project"": {{
					""fecha"": 1,
					""motivo"": 1,
					""diagnostico"": 1,
					""tratamiento"": 1,
					""observaciones"": 1,
					""IDMedico"": 1,
					""IDAlertas"": 1,
					""IDPaciente"": 1
				}}}}
			]";

        var bsonArray = BsonSerializer.Deserialize<BsonArray>(reports); // VAR
        var bsonDocuments = bsonArray.Select(stage => stage.AsBsonDocument).ToList();

        var pipeline = PipelineDefinition<Report, Report>.Create(bsonDocuments);// TASK

        var result = await _report.Aggregate(pipeline).ToListAsync();

        return result;
    }

	public async Task Create(Report report)
	{
		var last = await _report.Find(_ => true)
					   .SortByDescending(u => u.Id)
					   .Limit(1).FirstOrDefaultAsync();

		report.Id = last != null ? last.Id + 1 : 1;
		report.Date = report.Date.Date;

		await _report.InsertOneAsync(report);
	}
}

