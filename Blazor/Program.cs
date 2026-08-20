using Construction.Blazor.Components;
using Construction.Blazor.Core;
using Construction.Blazor.Core.Interop;
using Construction.Blazor.Core.Services;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

var builder = WebApplication.CreateBuilder(args);

var syncfusionLicenseKey = builder.Configuration["SyncfusionLicenseKey"];
if (!string.IsNullOrWhiteSpace(syncfusionLicenseKey))
{
    SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);
}

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSyncfusionBlazor();
builder.Services.AddMemoryCache(); // required by Syncfusion.Blazor.SfPdfViewer.SfPdfViewer2

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7070/api";
// In Development we accept the .NET self-signed HTTPS dev cert (the back-end
// runs on https://localhost:7070). In any other environment we fall back to
// the platform's default certificate validation, so a real CA-signed cert
// works without code changes.
if (builder.Environment.IsDevelopment())
{
    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
}
builder.Services.AddHttpClient("ConstructionApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl.EndsWith('/') ? apiBaseUrl : apiBaseUrl + "/");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment() &&
        apiBaseUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
    {
        // Trust the .NET dev cert so the Blazor server can call the
        // Construction.Api project (which uses the same dev cert).
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    return handler;
});

builder.Services.AddScoped<ApiClient>();
builder.Services.AddScoped<ReportsService>();
builder.Services.AddScoped<ProjectsService>();
builder.Services.AddScoped<RisksService>();
builder.Services.AddScoped<ChangeOrdersService>();
builder.Services.AddScoped<RiskMatrixService>();

builder.Services.AddScoped<ThemeInterop>();
builder.Services.AddScoped<DownloadInterop>();
builder.Services.AddScoped<TokenColorInterop>();
builder.Services.AddScoped<ChartPaletteService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
