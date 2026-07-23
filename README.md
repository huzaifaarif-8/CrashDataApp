# CrashDataApp

Full-stack airplane crash analytics dashboard. The .NET 8 Web API serves both the REST API and the Angular 21 frontend from the same origin on port 5050.

## Architecture

```
Browser → http://localhost:5050
          ├── /           → Angular SPA (served from wwwroot)
          ├── /api/crashes → REST API (12 endpoints)
          └── /swagger     → Swagger UI
```

**Backend:** ASP.NET Core 8 Web API with Entity Framework Core and SQLite.  
**Frontend:** Angular 21 standalone app, built into `wwwroot` so both run on the same port with no CORS issues.  
**Data:** ~5,268 rows loaded from a CSV file into SQLite on first run via EF Core + CsvHelper.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org) (only needed to rebuild the frontend)

## Running the app

The frontend is pre-built into `wwwroot`. To start the server:

```bash
cd CrashDataApp
dotnet restore
dotnet run --urls http://localhost:5050
```

Open `http://localhost:5050`. On first run EF Core creates `crashes.db` and seeds all rows from the CSV (takes a few seconds). Subsequent runs reuse the existing database.

To force a re-import, delete `crashes.db` and run again.

## Project structure

```
├── CrashDataApp/                   # .NET 8 Web API (backend)
│   ├── Controllers/
│   │   └── CrashesController.cs    # 12 REST endpoints
│   ├── Data/
│   │   ├── CrashContext.cs         # EF Core DbContext
│   │   ├── CsvImporter.cs          # seeds SQLite from CSV on first run
│   │   └── Airplane_Crashes_and_Fatalities_Since_1908.csv
│   ├── Models/
│   │   └── Crash.cs                # entity class (one row = one crash)
│   ├── wwwroot/                    # Angular build output (served as static files)
│   ├── Program.cs                  # app startup, middleware, DI
│   ├── appsettings.json            # connection string (defaults to crashes.db)
│   └── CrashDataApp.csproj
└── crash-dashboard/                # Angular 21 frontend (source)
    ├── src/
    │   ├── app/                    # component, template, styles, service
    │   └── styles.css              # global dark theme
    ├── angular.json                # builds output into CrashDataApp/wwwroot
    └── package.json
```

## API endpoints

All endpoints are under `/api/crashes`.

| Method | Path | Description |
|--------|------|-------------|
| GET | `/` | All crashes (paginated) |
| GET | `/{id}` | Single crash by ID |
| GET | `/summary` | Total crashes, fatalities, aboard, fatality rate |
| GET | `/by-decade` | Crashes and fatalities grouped by decade |
| GET | `/top-operators` | Top 10 operators by fatalities |
| GET | `/military-vs-civilian` | Fatality split by category |
| GET | `/top-aircraft-types` | Top 8 aircraft types by crash count |
| GET | `/engine-failure` | Years with most engine-failure mentions |
| GET | `/cumulative-fatalities` | Running fatality total over time |
| GET | `/year-over-year` | YoY % change in crash count (last 10 years) |
| GET | `/top-regions` | Top 10 regions by fatalities |
| GET | `/deadliest-per-decade` | Worst single crash in each decade |

Interactive docs at `http://localhost:5050/swagger`.

## How Entity Framework Core works here

1. `Crash.cs` defines the C# model — EF maps each property to a database column.
2. `CrashContext.cs` registers the `Crashes` table via `DbSet<Crash>`.
3. On startup, `EnsureCreated()` creates `crashes.db` (including the table) if it does not exist.
4. `CsvImporter.SeedIfEmpty()` checks whether the table is empty; if so, it reads every row from the CSV with CsvHelper and bulk-inserts them using `AddRange` + `SaveChanges`.
5. Controller endpoints query the database with LINQ — EF translates the LINQ expressions to SQL at runtime.

## Rebuilding the frontend

If you edit anything in `crash-dashboard/src/`:

```bash
cd crash-dashboard
npm install
npx @angular/cli@21 build --configuration development
```

The build output goes directly into `CrashDataApp/wwwroot` (configured in `angular.json`). Restart the .NET server to serve the updated files.
