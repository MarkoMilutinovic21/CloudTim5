# Smart Apiary

Smart Apiary je cloud platforma za upravljanje pčelinjacima i koordinaciju između pčelara i poljoprivrednika. Sistem omogućava praćenje stanja košnica putem IoT uređaja (pametnih vaga), upravljanje pčelinjacima i parcelama, kao i sistem upozorenja o pesticidima.

## Arhitektura

Sistem je zasnovan na Clean Architecture i Domain-Driven Design principima i sastoji se od sledećih slojeva:

- `SmartApiary.Domain` - domenski modeli i poslovna pravila
- `SmartApiary.Application` - CQRS komande i upiti (MediatR pattern)
- `SmartApiary.Infrastructure` - implementacije repozitorijuma (SQL Server, Azure Table Storage, Azure Blob Storage, Azure Queue Storage)
- `SmartApiary.WebApi` - ASP.NET Core Web API sa JWT autentifikacijom i SignalR
- `SmartApiary.Functions` - Azure Functions za IoT ingestion
- `SmartApiary.Simulator` - konzolna aplikacija za simulaciju IoT uređaja
- `dashboard` - React + TypeScript frontend

## Tehnologije

- .NET 8, ASP.NET Core Web API
- Entity Framework Core + Microsoft SQL Server LocalDB
- Azure Table Storage, Azure Blob Storage, Azure Queue Storage (Azurite lokalno)
- Azure Functions (Isolated Worker)
- SignalR (real-time komunikacija)
- React + TypeScript + Tailwind CSS
- JWT autentifikacija
- MediatR, FluentValidation
- SendGrid (slanje emailova)
- OpenWeather API (vremenska prognoza)
- Leaflet (mape)

## Preduslovi

- Visual Studio 2022 sa Azure development workloadom
- .NET 8 SDK
- Node.js i npm
- Azurite (lokalni Azure Storage emulator)
- Azure Functions Core Tools

Instalacija Azurite-a:

```bash
npm install -g azurite
```

## Pokretanje projekta

### 1. Pokreni Azurite

```bash
azurite
```

### 2. Pokreni backend

Pre prvog pokretanja postavi lokalne tajne (vrednosti ne treba commitovati):

```bash
dotnet user-secrets set "JwtSettings:Secret" "lokalni-jwt-kljuc-sa-najmanje-32-karaktera" --project SmartApiary.WebApi
dotnet user-secrets set "SeedAdmin:Email" "admin@smartapiary.local" --project SmartApiary.WebApi
dotnet user-secrets set "SeedAdmin:Password" "promeni-ovu-jaku-lozinku" --project SmartApiary.WebApi
dotnet user-secrets set "OpenWeatherMap:ApiKey" "OPENWEATHER_KLJUC" --project SmartApiary.WebApi
```

Otvori `SmartApiary.sln` u Visual Studio 2022 i pritisni `F5`.

Backend će automatski:

- kreirati SQL bazu podataka i pokrenuti migracije
- kreirati početnog administratora samo ako su `SeedAdmin` tajne podešene

### 3. Pokreni Azure Functions

U posebnom terminalu:

```bash
cd SmartApiary.Functions
func start --port 7071
```

### 4. Pokreni IoT simulator

U posebnom terminalu:

```bash
dotnet run --project SmartApiary.Simulator
```

Prijavljeni pčelar registruje serijski broj uređaja za svoju košnicu. Simulator automatski otkriva sve registrovane uređaje u lokalnom Azurite skladištu, izvršava handshake za nove uređaje i istovremeno šalje telemetriju za sve uparene košnice.

### 5. Pokreni frontend

```bash
cd dashboard
npm install
npm run dev
```

Frontend je dostupan na:

```text
http://localhost:5173
```

## Uloge u sistemu

- Administrator - upravljanje korisničkim nalozima (kreiranje, suspenzija, brisanje)
- Pčelar - upravljanje pčelinjacima i košnicama, telemetrija, upozorenja
- Poljoprivrednik - upravljanje parcelama, najava prskanja pesticidima

## Administratorski nalog

Podrazumevana javno poznata lozinka ne postoji. Razvojni administratorski nalog se opciono kreira iz `SeedAdmin:Email` i `SeedAdmin:Password` user-secrets vrednosti.

## Struktura projekta

```text
SmartApiary/
├── SmartApiary.Domain/
│   ├── Models/          # User, Apiary, Hive, Parcel...
│   └── Common/          # Entity, AggregateRoot, UserRoles
├── SmartApiary.Application/
│   ├── Features/        # CQRS komande i upiti po domenima
│   └── Common/
│       └── Interfaces/  # Repozitorijum interfejsi
├── SmartApiary.Infrastructure/
│   ├── Persistence/
│   │   ├── AppDbContext.cs
│   │   ├── Repositories/       # SQL repozitorijumi
│   │   └── AzureTable/         # Azure Table Storage repozitorijumi
│   └── Services/               # JwtService, EmailService
├── SmartApiary.WebApi/
│   ├── Controllers/
│   └── Program.cs
├── SmartApiary.Functions/       # Azure Functions
├── SmartApiary.Simulator/       # IoT simulator
└── dashboard/                   # React frontend
```

## API dokumentacija

Swagger UI je dostupan u development modu na:

```text
http://localhost:{port}/swagger
```
