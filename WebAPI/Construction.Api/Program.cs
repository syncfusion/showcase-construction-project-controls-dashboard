using Construction.Infrastructure.Data;
using Construction.Core.Interfaces;
using Construction.Infrastructure.Repositories;
using Construction.Infrastructure.Services;
using Construction.Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure PostgreSQL DbContext
builder.Services.AddDbContext<ConstructionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IRfiRepository, RfiRepository>();
builder.Services.AddScoped<ISubmittalRepository, SubmittalRepository>();
builder.Services.AddScoped<IInspectionRepository, InspectionRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IChangeOrderRepository, ChangeOrderRepository>();
builder.Services.AddScoped<IDocumentRepository, DocumentRepository>();
builder.Services.AddScoped<IRiskRepository, RiskRepository>();
builder.Services.AddScoped<IMilestoneRepository, MilestoneRepository>();

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IRfiService, RfiService>();
builder.Services.AddScoped<ISubmittalService, SubmittalService>();
builder.Services.AddScoped<IInspectionService, InspectionService>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<IChangeOrderService, ChangeOrderService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IRiskService, RiskService>();
builder.Services.AddScoped<IMilestoneService, MilestoneService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddHealthChecks();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// Add CORS for frontend access
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpaApps", policy =>
    {
        if (corsOrigins.Length == 0)
        {
            if (builder.Environment.IsDevelopment())
            {
                corsOrigins =
                [
                    "http://localhost:5173",
                    "http://127.0.0.1:5173",
                    "http://localhost:4200",
                    "http://127.0.0.1:4200"                ];
            }
            else
            {
                throw new InvalidOperationException(
                    "Production CORS origins are not configured.");
            }
        }

        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });

    //    // Per-IP rate limiter to mitigate trivial DoS on the public read-only showcase API. The
    //    // pageSize cap lives in QueryParametersDto (max 200). Together these bound the read surface.
    //    builder.Services.AddRateLimiter(options =>
    //    {
    //        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    //        {
    //            // Fall back to a single shared bucket if the remote IP cannot be determined (e.g. tests).
    //            var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "shared";
    //            return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
    //            {
    //                PermitLimit = 100,
    //                Window = TimeSpan.FromSeconds(60),
    //                QueueLimit = 0,
    //                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
    //            });
    //        });
    //        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    //    });
});
var app = builder.Build();

    // Configure the HTTP request pipeline.
    //if (app.Environment.IsDevelopment())
app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Construction API v1");
    options.RoutePrefix = string.Empty; // Swagger at root
});

// TODO: Database migrate + seed is an EXPLICIT, opt-in operation — it never runs automatically on
// app start. Set RUN_SEED=true (env var or the ForceSeed config key) for one start to apply
// pending EF Core migrations and (re)seed the deterministic showcase data. This keeps the
// default published behaviour read-only/idempotent and prevents the hosted demo from mutating
// its own schema or data on every boot. See README for usage.
//if (bool.TryParse(builder.Configuration["RUN_SEED"], out var runSeed) && runSeed
//    || bool.TryParse(builder.Configuration["ForceSeed"], out var forceSeed) && forceSeed)
//{
//    using var scope = app.Services.CreateScope();
//    var db = scope.ServiceProvider.GetRequiredService<ConstructionDbContext>();
//    await db.Database.MigrateAsync();
//    var seedForceFully = bool.TryParse(builder.Configuration["ForceSeed"], out var fs) && fs;
//    await DatabaseSeeder.SeedAsync(db, seedForceFully);
//}

//app.UseRateLimiter();
app.UseCors("AllowSpaApps");

app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () =>
{

    return Results.Ok(new
    {

        Status = "Healthy",

        Timestamp = DateTime.UtcNow,

        Environment = app.Environment.EnvironmentName,

        Version = "1.0.0"
    });

});
app.MapHealthChecks("/health");


await app.RunAsync();
