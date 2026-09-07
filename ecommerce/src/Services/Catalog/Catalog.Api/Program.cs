using System.Text;
using Catalog.Application;
using Catalog.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("CatalogDatabase");
var signingKey = builder.Configuration["Jwt:SigningKey"];
var issuer = builder.Configuration["Jwt:Issuer"] ?? "Ecommerce.Identity";
var audience = builder.Configuration["Jwt:Audience"] ?? "Ecommerce.Services";

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "La cadena de conexión 'CatalogDatabase' no está configurada.");
}

if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
{
    throw new InvalidOperationException(
        "Jwt:SigningKey debe configurarse externamente y tener al menos 32 caracteres.");
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ecommerce Catalog API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT emitido por Identity Service."
    });
});
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ICatalogRepository>(_ => new SqlCatalogRepository(connectionString));
builder.Services.AddScoped<CatalogService>();
builder.Services.AddHealthChecks()
    .AddCheck<CatalogDatabaseHealthCheck>("catalog-database");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)),
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = "sub",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization(options =>
    options.AddPolicy("AdministratorOnly", policy =>
        policy.RequireAuthenticatedUser().RequireRole("Administrator")));

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program;