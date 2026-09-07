using Catalog.Application;
using Catalog.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CatalogDatabase")
    ?? throw new InvalidOperationException("La cadena de conexión 'CatalogDatabase' no está configurada.");

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ICatalogRepository>(_ => new SqlCatalogRepository(connectionString));
builder.Services.AddScoped<CatalogService>();
builder.Services.AddHealthChecks()
    .AddCheck<CatalogDatabaseHealthCheck>("catalog-database");

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;
