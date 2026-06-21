using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using SmartApiary.Infrastructure;
using MediatR;
using FluentValidation;
using SmartApiary.Application.Features.Auth.Commands;
using SmartApiary.Domain.Models;
using SmartApiary.Domain.Common;
using SmartApiary.Infrastructure.Persistence;
using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using SmartApiary.WebApi.Hubs;
using SmartApiary.WebApi.Services;
using SmartApiary.WebApi;
using SmartApiary.Application.Common.Behaviors;
using SmartApiary.Application.Common.Interfaces;
using System.Security.Claims;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);
string jwtSecret = builder.Configuration["JwtSettings:Secret"] ?? string.Empty;
if (jwtSecret.Length < 32)
    throw new InvalidOperationException(
        "JwtSettings:Secret mora biti podešen kroz user-secrets ili environment i imati najmanje 32 karaktera.");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Unesite: Bearer {token}"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(LoginCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssemblyContaining<LoginCommand>();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
            ValidAudience = builder.Configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                string? accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    context.HttpContext.Request.Path.StartsWithSegments("/telemetryhub"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                string? userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdValue, out Guid userId))
                {
                    context.Fail("Token ne sadrži ispravan identitet korisnika.");
                    return;
                }

                IUserRepository users = context.HttpContext.RequestServices
                    .GetRequiredService<IUserRepository>();
                User? user = await users.GetByIdAsync(userId, context.HttpContext.RequestAborted);
                if (user is null || !user.IsActive)
                    context.Fail("Korisnički nalog je suspendovan ili obrisan.");
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSignalR();
builder.Services.AddHostedService<TelemetryBroadcastService>();
builder.Services.AddHostedService<SprayingRecordAutomationService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

    options.AddPolicy("SignalR", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<TelemetryHub>("/telemetryhub").RequireAuthorization().RequireCors("SignalR");

using (var migrationScope = app.Services.CreateScope())
{
    var database = migrationScope.ServiceProvider.GetRequiredService<AppDbContext>();
    await database.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    string? adminEmail = builder.Configuration["SeedAdmin:Email"];
    string? adminPassword = builder.Configuration["SeedAdmin:Password"];
    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        User? adminUser = await database.Users.SingleOrDefaultAsync(user => user.Email == adminEmail);

        if (adminUser is null)
        {
            adminUser = User.Create(
                "Admin", "Admin", adminEmail,
                BCrypt.Net.BCrypt.HashPassword(adminPassword), UserRoles.Admin);
            adminUser.Activate();
            database.Users.Add(adminUser);
        }
        else
        {
            if (adminUser.Role != UserRoles.Admin)
                throw new InvalidOperationException("SeedAdmin email pripada nalogu koji nije administrator.");

            bool passwordMatches;
            try
            {
                passwordMatches = BCrypt.Net.BCrypt.Verify(adminPassword, adminUser.PasswordHash);
            }
            catch
            {
                passwordMatches = false;
            }

            if (!passwordMatches)
                adminUser.SetPassword(BCrypt.Net.BCrypt.HashPassword(adminPassword));

            if (!adminUser.IsActive)
                adminUser.Activate();
        }

        await database.SaveChangesAsync();
    }
}

app.Run();
