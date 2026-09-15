using System.Text;
using Cart.Application;
using Cart.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("CartDatabase");
var signingKey = builder.Configuration["Jwt:SigningKey"];
var issuer = builder.Configuration["Jwt:Issuer"] ?? "Ecommerce.Identity";
var audience = builder.Configuration["Jwt:Audience"] ?? "Ecommerce.Services";

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("La cadena de conexión 'CartDatabase' no está configurada.");
if (string.IsNullOrWhiteSpace(signingKey) || signingKey.Length < 32)
    throw new InvalidOperationException("Jwt:SigningKey debe configurarse externamente y tener al menos 32 caracteres.");

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddSingleton<ICartRepository>(_ => new SqlCartRepository(connectionString));
builder.Services.AddScoped<CartService>();
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
    options.AddPolicy("CustomerOnly", policy =>
        policy.RequireAuthenticatedUser().RequireRole("Customer")));
builder.Services.AddHealthChecks().AddCheck<CartDatabaseHealthCheck>("cart-database");

var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.Run();

public partial class Program;
