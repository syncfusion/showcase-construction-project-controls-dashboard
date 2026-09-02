# MAUI API Configuration Guide

## Current Setup

The MAUI project is configured to connect to the ASP.NET Core Construction API.

### API Endpoints

- **HTTPS (Secure)**: `https://localhost:7070/api`
- **HTTP (Dev)**: `http://localhost:5228/api`

### Current Configuration

**File**: `appsettings.json`
```json
{
  "ApiBaseUrl": "https://localhost:7070/api",
  "SyncfusionLicense": "..."
}
```

## How It Works

1. **MauiProgram.cs** reads `appsettings.json` and configures the HTTP client
2. **ApiClient.cs** provides a thin JSON wrapper with automatic serialization
3. All ViewModels inject `IApiClient` and call service methods
4. Services (ProjectsService, RisksService, ReportsService) call the API endpoints

### Flow Diagram

```
MAUI UI (Dashboard, Projects, etc.)
    ↓
ViewModel (DashboardViewModel, ProjectsViewModel, etc.)
    ↓
Service Layer (ProjectsService, RisksService, IReportsService)
    ↓
ApiClient (HTTP wrapper with JSON serialization)
    ↓
HttpClient (IHttpClientFactory named "ConstructionApi")
    ↓
ASP.NET Core API (localhost:7070 or localhost:5228)
```

## Switching Between Endpoints

### Option 1: Use HTTPS (Current)
Edit `appsettings.json`:
```json
"ApiBaseUrl": "https://localhost:7070/api"
```

### Option 2: Use HTTP
Edit `appsettings.json`:
```json
"ApiBaseUrl": "http://localhost:5228/api"
```

## SSL Certificate Handling

For **localhost HTTPS testing in DEBUG builds**, the project automatically:
- Bypasses SSL certificate validation (development only)
- Configured in `MauiProgram.cs` with `#if DEBUG` guards

**Never ship this to production** — production builds use strict certificate validation.

## Testing the Connection

1. **Ensure API is running**:
   ```bash
   # Terminal 1: Start the API
   cd backend/Construction.Api
   dotnet run
   ```

2. **Run the MAUI app**:
   ```bash
   cd frontend/Construction.Maui
   dotnet run -f net10.0-windows10.0.19041.0
   ```

3. **Monitor API calls** in the console output or network inspection tools

## Troubleshooting

### API Connection Fails
- ✅ Verify API is running on the configured port
- ✅ Check firewall isn't blocking localhost connections
- ✅ Verify `appsettings.json` has correct URL
- ✅ Check browser console for CORS errors (if cross-origin)

### SSL Certificate Errors
- ✅ For localhost HTTPS: debug builds auto-bypass (see MauiProgram.cs)
- ✅ For production: ensure valid certificate or use HTTP

### JSON Serialization Issues
- ✅ ApiClient uses `JsonSerializerOptions` from `MauiProgram.JsonOptions`
- ✅ Settings: PropertyNameCaseInsensitive=true, PropertyNamingPolicy=CamelCase
- ✅ Enums serialized as strings (not numbers)

## Key Services

| Service | Purpose | Endpoint |
|---------|---------|----------|
| `IApiClient` | HTTP wrapper | Core HTTP operations |
| `IProjectsService` | Project data | `/projects`, `/projects/{id}` |
| `IRisksService` | Risk data | `/risks`, `/risks/{id}` |
| `IReportsService` | Reports/KPIs | `/reports/kpis`, `/reports/earned-value` |

## Files Modified

- ✅ `appsettings.json` — API URL configuration
- ✅ `MauiProgram.cs` — HTTP client setup with SSL bypass for DEBUG
- ✅ `Services/ApiClient.cs` — Core HTTP wrapper (pre-existing)
- ✅ `Services/*.Service.cs` — Business logic layers (pre-existing)
