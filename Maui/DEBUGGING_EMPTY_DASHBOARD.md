# Debugging Empty Dashboard Issue

## Quick Diagnosis

If the Dashboard appears empty, follow these steps:

### Step 1: Verify API is Running

Check if the backend is running on the configured endpoint:

```bash
# Terminal 1: Start API
cd backend/Construction.Api
dotnet run

# You should see output like:
# info: Microsoft.Hosting.Lifetime[14]
#       Now listening on: https://localhost:7070
#       Now listening on: http://localhost:5228
```

### Step 2: Test API Endpoints Directly

Open your browser and test:
- `https://localhost:7070/api/reports/portfolio-kpis`
- `http://localhost:5228/api/reports/portfolio-kpis`

**Expected response**: JSON data, NOT 404 or 500 error

### Step 3: Check MAUI Logs in Debug Output

When running the MAUI app in debug mode:

```bash
cd frontend/Construction.Maui
dotnet run -f net10.0-windows10.0.19041.0
```

**Look in the Debug output console for**:
- `GET https://localhost:7070/api/reports/...` — Request being made
- `GET https://localhost:7070/api/reports/... → 200` — Success
- `GET https://localhost:7070/api/reports/... failed` — Error (with details)

### Step 4: Verify API Configuration

Check `appsettings.json`:

```json
{
  "ApiBaseUrl": "https://localhost:7070/api",
  ...
}
```

If using HTTP instead, change to:
```json
{
  "ApiBaseUrl": "http://localhost:5228/api",
  ...
}
```

Then **rebuild** the app.

---

## Common Issues & Fixes

| Symptom | Cause | Fix |
|---------|-------|-----|
| **Dashboard is blank** | API not responding | Start backend with `dotnet run` |
| **"Connection refused" error** | Wrong API URL | Check `appsettings.json` ApiBaseUrl |
| **"SSL certificate error"** | HTTPS localhost cert invalid | Already handled by DEBUG mode (see MauiProgram.cs) |
| **Data loads but doesn't display** | UI binding issue | Check DashboardPage XAML binding properties |
| **API returns 404** | Endpoint doesn't exist | Verify backend has `/api/reports/*` endpoints |
| **API returns 500** | Backend exception | Check backend console for error message |

---

## Data Flow Verification

```
1. DashboardPage loads
   ↓
2. AppShellLayout.SetContent() calls vm.LoadAsync()
   ↓
3. DashboardViewModel.LoadAsync() calls _reports service
   ↓
4. ReportsService calls _api.GetJsonAsync("reports/portfolio-kpis")
   ↓
5. ApiClient sends GET request to https://localhost:7070/api/reports/portfolio-kpis
   ↓
6. Backend processes request and returns JSON
   ↓
7. Data binds to Dashboard UI
```

If any step fails, you'll see:
- **Nothing**: Step 1 (page not loading)
- **Blank dashboard**: Step 6 (no data returned)
- **Error message**: ViewModelBase.RunSafeAsync caught an exception (see ErrorMessage property)

---

## Enable Verbose Logging

To see **every HTTP request**, the logging level in MauiProgram.cs is already set to `Debug` in DEBUG builds:

```csharp
#if DEBUG
    builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif
```

Check the Visual Studio Debug Output window to see logs like:
```
info: Construction.Maui.Services.ApiClient[0]
      GET https://localhost:7070/api/reports/portfolio-kpis
info: Construction.Maui.Services.ApiClient[0]
      GET https://localhost:7070/api/reports/portfolio-kpis → 200 OK
```

---

## Dashboard Should Display

Once data loads successfully, you should see:

- ✅ **4 KPI cards**: Active Projects, Schedule Variance, Cost Variance, Critical Risks
- ✅ **Cost Performance Trend chart**: 6-month line chart
- ✅ **Portfolio Health donut**: On Track / At Risk / Critical / Not Started breakdown
- ✅ **Upcoming Milestones list**: Next 10 milestones (14-day horizon)

If any section is missing after data loads, check the XAML binding paths in `DashboardPage.xaml`.

---

## Get Help

If still stuck:
1. **Paste the Debug output** (from Visual Studio or terminal)
2. **Verify backend is running** with the two API URLs
3. **Test a direct API call** in browser
4. **Check for exceptions** in ViewModelBase.ErrorMessage (displayed on screen)
