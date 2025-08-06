using Microsoft.Extensions.DependencyInjection.Extensions;
using SenseCareLocal.Config;
using SenseCareLocal.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDBSettings>(
    builder.Configuration.GetSection("MongoDBSettings"));

builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<PatientService>();
builder.Services.AddSingleton<DeviceService>();
builder.Services.AddSingleton<VitalSignsService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
