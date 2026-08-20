# Construction Project Controls Dashboard

Construction Project Controls Dashboard is a cross-framework enterprise showcase application built with a shared ASP.NET Core Web API and three Syncfusion-powered clients: Angular, React, and Blazor.

The application delivers a centralized command center for construction project controls — integrating project management, scheduling, cost control, and field reporting data into one real-time view. Project managers, planners, and stakeholders can track progress, monitor budgets, assess risks, and identify deviations from planned baselines across a portfolio of projects. It also shows how the same enterprise experience can be delivered consistently across different frontend stacks by using Syncfusion UI components.

> This is a showcase application with deterministic sample data. It is not intended to be used as a production system without adding production authentication, authorization, auditing, secrets management, and operational controls.

## What the showcase includes

- Executive dashboard with project health, KPIs, progress %, and milestone status
- Project portfolio management with drill-down by project, phase, and contractor
- Cost tracking (budget vs. actuals, forecasts, cost variance by cost code) and change order management
- Schedule tracking with a Gantt-style task view (planned vs. actual timelines, delays) and an activities calendar
- Earned Value Management (EVM) metrics — cost performance and schedule performance indicators
- Risk and issue tracking with severity, probability, and status indicators, plus a risk matrix
- RFI (Requests for Information) and submittal workflows
- Inspections tracking and a document workspace
- Site location map view
- Historical trends and reporting
- Alerts for deviations such as cost overruns or schedule delays
- Responsive layouts, loading states, error handling, and accessible navigation
- Swagger/OpenAPI documentation and deterministic PostgreSQL seed data

## Technology

| Layer | Technology |
| --- | --- |
| API | ASP.NET Core Web API on .NET 10 |
| Data | Entity Framework Core and PostgreSQL |
| Angular client | Angular 22, TypeScript, RxJS, Syncfusion Angular UI, Lucide icons |
| React client | React 19, TypeScript, Vite, Syncfusion React UI, Lucide icons |
| Blazor client | Blazor (Interactive Server) on .NET 10, Syncfusion Blazor UI |
| Testing | .NET xUnit test project |

The UI implementations use Syncfusion components such as DataGrid, Charts, Gantt/Scheduler, Maps, Diagrams, HeatMap, and PDF Viewer.

## Why Syncfusion UI components?

This repository is a practical proof of how the [Syncfusion component ecosystem](https://www.syncfusion.com/) can accelerate component-rich enterprise development.

- Production-oriented UI components reduce the amount of custom code required for advanced grids, charts, scheduling, Gantt views, maps, and document viewing.
- Similar component concepts across Angular, React, and Blazor make it easier to preserve business behavior while choosing the framework that best fits each team.
- Built-in capabilities such as filtering, grouping, export, responsive rendering, accessibility, and theming help teams focus on business workflows rather than foundational UI infrastructure.

## Repository structure

```text
showcase-git/
├── WebAPI/                                  # .NET solution + 4 backend projects
│   ├── ConstructionProjectControls.slnx     # Solution file
│   ├── Construction.Api/                     # ASP.NET Core Web API, controllers, Swagger
│   ├── Construction.Core/                    # Entities, DTOs, and service/repository interfaces
│   ├── Construction.Infrastructure/        # EF Core DbContext, migrations, repositories, services, seed data
│   └── Construction.Tests/                   # xUnit test project
├── Angular/                                 # Angular client
├── React/                                   # React client (Vite)
└── Blazor/                                  # Blazor Interactive Server client (.NET 10)
```

Each frontend is a standalone application with its own `README.md`, `package.json` (or `.csproj`), and build configuration.

## Run locally

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- A current Node.js LTS release and npm
- PostgreSQL
- A valid Syncfusion license or trial where required (registered in each client's startup code)

Create a PostgreSQL database named `ConstructionProjectControls`, then update the connection string in:

```text
WebAPI/Construction.Api/appsettings.Development.json
```

### 1. Start the Web API

```bash
cd WebAPI
dotnet restore ConstructionProjectControls.slnx
dotnet run --project Construction.Api/Construction.Api.csproj
```

The API runs at `http://localhost:5228` (and `https://localhost:7070`), with Swagger UI at the root URL.

The API exposes a **read-only** surface (GET endpoints only). Create / Update / Delete actions are intentionally omitted from the controllers and service layer so the public showcase cannot be used to insert, update, or delete data — this keeps the demo dataset stable and avoids anonymous write/delete vectors (data tampering, denial of service). The client apps' "New ..."/"Save ..." modals mutate local view state only; nothing is sent to the backend.

Database migrations and deterministic showcase seeding are **explicit, opt-in** steps — they do not run automatically on app start. To apply pending EF Core migrations and (re)seed the showcase data for one run, set `RUN_SEED=true`:

```bash
# Windows (PowerShell)
$env:RUN_SEED="true"; dotnet run --project Construction.Api/Construction.Api.csproj
# Linux / macOS
RUN_SEED=true dotnet run --project Construction.Api/Construction.Api.csproj
```

Set the PostgreSQL connection string via [user secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or an environment variable rather than committing a real password:

```bash
cd WebAPI/Construction.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Database=ConstructionProjectControls;Username=postgres;Password=<your-password>"
```

The API also applies a per-IP rate limit (100 requests / 60 seconds) and caps `pageSize` at 200 to bound the read surface against trivial DoS.

### 2. Start a frontend

Angular (runs on port `4200`, proxies API requests via `proxy.conf.json`):

```bash
cd Angular
npm install
npm start
```

Open `http://localhost:4200`.

React (reads `VITE_API_BASE_URL` from `.env.development`, runs on port `5173`):

```bash
cd React
npm install
npm run dev
```

Open `http://localhost:5173`.

Blazor (Interactive Server, .NET 10; reads `ApiBaseUrl` from `appsettings.json`):

```bash
cd Blazor
dotnet run
```

Open `http://localhost:5023`.

## Build and test

```bash
# Backend
cd WebAPI
dotnet build ConstructionProjectControls.slnx
dotnet test Construction.Tests/Construction.Tests.csproj

# Angular
cd ../Angular
npm run build

# React
cd ../React
npm run build

# Blazor
cd ../Blazor
dotnet build
```

## Licensing

Syncfusion packages are governed by Syncfusion's licensing terms. Review the [Syncfusion licensing documentation](https://www.syncfusion.com/sales/licensing) before redistributing or deploying the applications. Publishing this source repository does not grant a license to Syncfusion products.

## Intended audience

This showcase is useful for engineering leaders, architects, product teams, and developers evaluating how a construction project controls system can be implemented with a shared API and multiple enterprise web frameworks.
