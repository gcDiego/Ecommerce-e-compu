using System.Text;
using System.Text.Json.Serialization;
using Identity.Application;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("IdentityDatabase");
var signingKey = builder.Configuration["Jwt:SigningKey"];
var issuer = builder.Configuration["Jwt:Issuer"] ?? "Ecommerce.Identity";
var audience = builder.Configuration["Jwt:Audience"] ?? "Ecommerce.Services";

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("La cadena de conexión 'IdentityDatabase' no está configurada.");
if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
    throw new InvalidOperationException("Jwt:SigningKey debe configurarse externamente y tener al menos 32 caracteres.");

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
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
builder.Services.AddAuthorization();
builder.Services.AddSingleton<IIdentityRepository>(_ => new SqlIdentityRepository(connectionString));
builder.Services.AddSingleton<IPasswordVerifier, LegacySha256PasswordVerifier>();
builder.Services.AddSingleton<ITokenIssuer>(_ => new JwtTokenIssuer(
    issuer,
    audience,
    signingKey,
    TimeSpan.FromMinutes(30)));
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<PasswordChangeService>();
builder.Services.AddHealthChecks().AddCheck<IdentityDatabaseHealthCheck>("identity-database");

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

public partial class Program;
