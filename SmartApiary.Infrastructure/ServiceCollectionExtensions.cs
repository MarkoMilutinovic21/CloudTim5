namespace SmartApiary.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartApiary.Application.Common.Interfaces;
using SmartApiary.Infrastructure.External;
using SmartApiary.Infrastructure.Persistence;
using SmartApiary.Infrastructure.Persistence.Repositories;
using SmartApiary.Infrastructure.Persistence.AzureTable.Repositories;
using SmartApiary.Infrastructure.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IJwtService, JwtService>();

        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        services.Configure<AzureTableOptions>(
            configuration.GetSection("AzureTableOptions"));

        services.Configure<AzureBlobOptions>(
            configuration.GetSection("AzureBlobOptions"));

        services.AddScoped<IApiaryRepository, ApiaryRepository>();
        services.AddScoped<IHiveRepository, HiveRepository>();
        services.AddScoped<IHiveJournalEntryRepository, HiveJournalEntryRepository>();
        services.AddScoped<IParcelRepository, ParcelRepository>();
        services.AddScoped<IPesticideTreatmentRepository, PesticideTreatmentRepository>();
        services.AddScoped<IDeviceRepository, DeviceRepository>();
        services.AddScoped<ITelemetryMeasurementRepository, TelemetryMeasurementRepository>();
        services.AddScoped<IBeekeeperAlertRepository, BeekeeperAlertRepository>();

        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IApiaryImageStorage, ApiaryImageStorage>();
        services.AddScoped<ICropRepository, CropRepository>();
        services.AddScoped<ISprayingRecordRepository, SprayingRecordRepository>();

        services.AddHttpClient<IWeatherService, OpenWeatherMapService>();

        return services;
    }
}
