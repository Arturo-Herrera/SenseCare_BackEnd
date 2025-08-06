using SenseCareLocal.Config;
using SenseCareLocal.DataAccess;
using SenseCareLocal.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<PatientService>();
builder.Services.AddSingleton<DeviceService>();
builder.Services.AddSingleton<VitalSignsService>();
builder.Services.AddSingleton<AlertService>();
builder.Services.AddSingleton<ReportService>();
builder.Services.AddSingleton<MapService>();
builder.Services.AddSingleton<TriggerDevice>();


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
app.UseCors("CorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
