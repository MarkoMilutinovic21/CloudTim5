using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Functions.Options;
using SmartApiary.Infrastructure;
using SmartApiary.Infrastructure.Persistence.AzureTable.Repositories;
using System.Text.Json;

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

builder.Services.AddScoped<IDeviceRepository, DeviceRepository>();
builder.Services.AddScoped<ITelemetryMeasurementRepository, TelemetryMeasurementRepository>();

builder.Build().Run();
