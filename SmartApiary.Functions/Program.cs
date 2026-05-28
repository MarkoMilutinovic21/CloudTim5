using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Functions.Options;
using SmartApiary.Infrastructure;
using SmartApiary.Infrastructure.Persistence;
using SmartApiary.Infrastructure.Persistence.Repositories;
using SmartApiary.Infrastructure.Persistence.AzureTable.Repositories;
using SmartApiary.Infrastructure.Services;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.Configure<AzureQueueOptions>(
    builder.Configuration.GetSection(nameof(AzureQueueOptions)));

builder.Services.Configure<AzureTableOptions>(
    builder.Configuration.GetSection(nameof(AzureTableOptions)));

// Pokušaj oba na?ina u?itavanja - ako je jedan null, iskoristi drugi.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"] 
    ?? Environment.GetEnvironmentVariable("ConnectionStrings:DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        connectionString,
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<ITelemetryMeasurementRepository, TelemetryMeasurementRepository>();
builder.Services.AddScoped<IApiaryRepository, ApiaryRepository>();
builder.Services.AddScoped<IHiveRepository, HiveRepository>();
builder.Services.AddScoped<IBeekeeperAlertRepository, BeekeeperAlertRepository>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Build().Run();
