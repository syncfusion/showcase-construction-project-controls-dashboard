using System.Text.Json;
using System.Text.Json.Serialization;
using Construction.Maui.Services;
using Construction.Maui.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Syncfusion.Licensing;
using Syncfusion.Maui.Core.Hosting;

namespace Construction.Maui;

public static class MauiProgram
{
    public const string ApiHttpClientName = "ConstructionApi";

    // Same sample PDF used by the React/Blazor ports for the Documents previewer.
    public const string SamplePdfUrl = "https://cdn.syncfusion.com/content/pdf/pdf-succinctly.pdf";

    public static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder()
            .UseMauiApp<App>();

        // ---- Configuration (appsettings.json) ----
        // Two resolution paths depending on how the app is launched:
        //
        //   • Packaged (MSIX / Android / iOS): the file lives inside the app
        //     package and is read via FileSystem.OpenAppPackageFileAsync — the
        //     only reliable way to reach bundled assets on those platforms.
        //
        //   • Unpackaged Windows (dotnet run / double-clicking the .exe):
        //     OpenAppPackageFileAsync on unpackaged Windows looks up files
        //     relative to the .exe working directory. To guarantee the file is
        //     found regardless of the SDK's copy behavior, we ALSO probe
        //     AppContext.BaseDirectory (where the .exe lives) directly. The
        //     csproj's <None> item copies appsettings.json there on build.
        //
        // If the file is genuinely missing (e.g. a stripped manual deploy),
        // fall back to the public showcase API instead of crashing so the app
        // still launches and shows a window.
        var loadedConfig = false;
        try
        {
            // Prefer the file sitting next to the .exe (unpackaged Windows).
            var exeDir = AppContext.BaseDirectory;
            var exePath = Path.Combine(exeDir, "appsettings.json");
            if (File.Exists(exePath))
            {
                using var fs = File.OpenRead(exePath);
                builder.Configuration.AddJsonStream(fs);
                loadedConfig = true;
            }
            else
            {
                // Fall back to the bundled asset (packaged runs / mobile).
                using var stream = FileSystem.OpenAppPackageFileAsync("appsettings.json").GetAwaiter().GetResult();
                builder.Configuration.AddJsonStream(stream);
                loadedConfig = true;
            }
        }
        catch (Exception ex)
        {
            _ = ex;
        }

        if (!loadedConfig)
        {
            // No appsettings.json found anywhere — use the public showcase API
            // so the app still launches with a live data source.
            builder.Configuration["ApiBaseUrl"] = "https://construction-api-showcase.syncfusion.com/api";
        }

        // ---- Fonts (Lucide icon font exposed alongside OpenSans) ----
        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            fonts.AddFont("lucide.ttf", "Lucide");
        });

        // ---- Syncfusion MAUI license + core hosting ----
        var licenseKey = builder.Configuration["SyncfusionLicense"];
        if (!string.IsNullOrWhiteSpace(licenseKey))
        {
            SyncfusionLicenseProvider.RegisterLicense(licenseKey);
        }
        builder.ConfigureSyncfusionCore();

        // ---- HTTP client pointed at the existing ASP.NET Core API ----
        var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5228/api";
        // On the Android emulator, the host OS is reached via the special alias 10.0.2.2
        // (localhost / 127.0.0.1 there refer to the emulator's own loopback). Rewrite the
        // configured base URL before handing it to the HttpClient so dev API calls succeed.
#if ANDROID
        apiBaseUrl = apiBaseUrl
            .Replace("://localhost", "://10.0.2.2")
            .Replace("://127.0.0.1", "://10.0.2.2");
#endif
        builder.Services.AddHttpClient(ApiHttpClientName, client =>
        {
            client.BaseAddress = new Uri(apiBaseUrl.EndsWith('/') ? apiBaseUrl : apiBaseUrl + "/");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            // 20s — short enough that a dead/slow endpoint fails fast instead of
            // stalling the UI for minutes (the previous 60s default combined with
            // an IPv6-first DNS resolution stall caused ~390s hangs against the
            // live API, which crashed the app via TaskCanceledException before the
            // window could render — see the SocketsHttpHandler IPv4 fix below).
            client.Timeout = TimeSpan.FromSeconds(20);
        })
        // Force the handler to resolve IPv4 first. The default HttpClientHandler
        // tries IPv6 then waits out the full timeout before falling back to IPv4,
        // which against some hosts (incl. the live showcase API) produced ~390s
        // stalls and TaskCanceledException crashes. SocketsHttpHandler lets us pin
        // the address family so the first connect attempt uses IPv4 directly.
        .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            // IPv4-first: avoid the IPv6 fallback stall that was hanging the app.
            // (HttpClientHandler has no AddressFamily hook; SocketsHttpHandler does.)
            ConnectCallback = async (context, ct) =>
            {
                // Resolve and prefer the IPv4 endpoint for the host.
                var host = context.DnsEndPoint.Host;
                var port = context.DnsEndPoint.Port;
                System.Net.IPAddress[] addrs;
                try
                {
                    addrs = await System.Net.Dns.GetHostAddressesAsync(host, System.Net.Sockets.AddressFamily.InterNetwork, ct).ConfigureAwait(false);
                }
                catch
                {
                    addrs = await System.Net.Dns.GetHostAddressesAsync(host, ct).ConfigureAwait(false);
                }
                var ipv4 = addrs.FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) ?? addrs.FirstOrDefault();
                if (ipv4 is null)
                    throw new System.Net.Sockets.SocketException((int)System.Net.Sockets.SocketError.HostNotFound, $"No IP address found for {host}");

                var tcp = new System.Net.Sockets.TcpClient();
                try
                {
                    await tcp.ConnectAsync(ipv4, port, ct).ConfigureAwait(false);
                    return tcp.GetStream();
                }
                catch
                {
                    tcp.Dispose();
                    throw;
                }
            }
        });

        builder.Services.AddSingleton(JsonOptions);

        // ---- Logging ----
#if DEBUG
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
#endif

        // ---- Service layer ----
        // The dashboard now reads pre-extracted JSON data from the Data/ folder
        // (via LocalDataService) instead of calling the backend API at runtime,
        // so it showcases real data WITHOUT the API/DB running. The original
        // HTTP-backed services (ApiClient, ReportsService, ProjectsService,
        // RisksService) are kept below as commented registrations — restore
        // them and remove the Local* ones to switch back to live API calls.
        builder.Services.AddSingleton<IApiClient, ApiClient>();
        builder.Services.AddSingleton<ILocalDataService, LocalDataService>();
        builder.Services.AddSingleton<IProjectsService, LocalProjectsService>();
        builder.Services.AddSingleton<IReportsService, LocalReportsService>();
        builder.Services.AddSingleton<IRisksService, LocalRisksService>();
        // ---- Previous HTTP-backed service registrations (preserved — restore to use the live API) ----
        // builder.Services.AddSingleton<IProjectsService, ProjectsService>();
        // builder.Services.AddSingleton<IReportsService, ReportsService>();
        // builder.Services.AddSingleton<IRisksService, RisksService>();

        // ---- ViewModels (transient — fresh instance per navigation) ----
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<ProjectsViewModel>();
        builder.Services.AddTransient<ProjectDetailViewModel>();
        builder.Services.AddTransient<CostControlViewModel>();
        builder.Services.AddTransient<RisksViewModel>();
        builder.Services.AddTransient<ReportsViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();
        App.Services = app.Services;
        return app;
    }
}
