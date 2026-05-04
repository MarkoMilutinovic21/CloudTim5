namespace SmartApiary.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmartApiary.Application.Common.Interfaces;
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
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddSingleton<IJwtService, JwtService>();

        services.Configure<JwtSettings>(
            configuration.GetSection("JwtSettings"));

        services.Configure<AzureTableOptions>(
            configuration.GetSection("AzureTableOptions"));

        services.AddScoped<IApiaryRepository, ApiaryRepository>();
        services.AddScoped<IHiveRepository, HiveRepository>();
        services.AddScoped<IParcelRepository, ParcelRepository>();
        services.AddScoped<IPesticideTreatmentRepository, PesticideTreatmentRepository>();

        services.AddScoped<IEmailService, EmailService>();

        return services;
    }
}