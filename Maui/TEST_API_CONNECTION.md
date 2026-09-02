# Test API Connection (Quick Reference)

## 1️⃣ Verify Backend is Running

```bash
cd backend/Construction.Api
dotnet run
```

Expected output:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7070
      Now listening on: http://localhost:5228
```

## 2️⃣ Test Each Endpoint

### Option A: Browser (Easiest)

Open these URLs in a browser:

| Endpoint | URL | Expected |
|----------|-----|----------|
| Portfolio KPIs | `https://localhost:7070/api/reports/portfolio-kpis` | JSON with `activeProjects`, `scheduleVariancePct`, etc. |
| Cost KPIs | `https://localhost:7070/api/reports/cost-kpis` | JSON with `totalBudget`, `committedSpend`, etc. |
| Health Distribution | `https://localhost:7070/api/reports/project-health-distribution` | JSON with `onTrack`, `atRisk`, `critical`, `notStarted` counts |
| Cost Trend (6 mo) | `https://localhost:7070/api/reports/cost-performance-trend?months=6` | JSON array of points with `period`, `planned`, `actual` |
| Milestones (14 days) | `https://localhost:7070/api/reports/upcoming-milestones?days=14&limit=10` | JSON array of milestones |

### Option B: PowerShell (If browser fails)

```powershell
# Test HTTPS endpoint
Invoke-RestMethod -Uri "https://localhost:7070/api/reports/portfolio-kpis" -SkipCertificateCheck | ConvertTo-Json

# Test HTTP endpoint
Invoke-RestMethod -Uri "http://localhost:5228/api/reports/portfolio-kpis" | ConvertTo-Json
```

### Option C: curl (If bash available)

```bash
# Test HTTPS (skip cert check)
curl -k https://localhost:7070/api/reports/portfolio-kpis

# Test HTTP
curl http://localhost:5228/api/reports/portfolio-kpis
```

---

## 3️⃣ Common Responses

### ✅ Success (200 OK)
```json
{
  "activeProjects": 12,
  "scheduleVariancePct": 5.2,
  "costVariancePct": -3.1,
  "criticalRisksCount": 2,
  "cpi": 0.98,
  "spi": 1.02
}
```

### ❌ Not Running (Connection Refused)
- **Error**: "Cannot connect to server"
- **Fix**: Start backend with `dotnet run`

### ❌ Wrong URL (404 Not Found)
- **Error**: 404
- **Fix**: Check URL spelling, verify `/api/reports/` paths exist in backend

### ❌ SSL Error (SSL Certificate Problem)
- **Error**: "SSL certificate problem"
- **Fix**: Use HTTP (`localhost:5228`) OR skip cert check (`-k` in curl, `SkipCertificateCheck` in PowerShell)

---

## 4️⃣ Run MAUI App

Once endpoints respond successfully:

```bash
cd frontend/Construction.Maui
dotnet run -f net10.0-windows10.0.19041.0
```

**Check Debug Output** for:
```
info: Construction.Maui.Services.ApiClient[0]
      GET https://localhost:7070/api/reports/portfolio-kpis
info: Construction.Maui.Services.ApiClient[0]
      GET https://localhost:7070/api/reports/portfolio-kpis → 200
```

Dashboard should populate with KPIs, charts, and milestones! ✅

---

## 5️⃣ If Dashboard Still Empty

Check Dashboard UI bindings in `DashboardPage.xaml`:

```xaml
<!-- These should show data if API is working -->
<Label Text="{Binding Portfolio.ActiveProjects}" />
<Label Text="{Binding ScheduleVarianceDisplay}" />

<!-- These collections should populate -->
<CollectionView ItemsSource="{Binding Milestones}" />
<SfCircularChart ItemsSource="{Binding HealthSegments}" />
```

If API responds but UI is empty, the issue is in XAML binding paths (property names don't match).

---

## Summary Checklist

- [ ] Backend running on https://localhost:7070 or http://localhost:5228
- [ ] API endpoints respond with JSON in browser
- [ ] appsettings.json has correct ApiBaseUrl
- [ ] MAUI app started with `dotnet run -f net10.0-windows10.0.19041.0`
- [ ] Debug output shows successful GET requests (→ 200)
- [ ] Dashboard displays KPIs, charts, and milestones
