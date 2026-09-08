using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Catalog.Api.Models;
using Catalog.Application;
using Catalog.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace Catalog.IntegrationTests;

public sealed class CatalogApiTests : IClassFixture<CatalogApiFactory>
{
    private readonly HttpClient client;
    private const string SigningKey = "test-signing-key-with-at-least-32-characters";

    public CatalogApiTests(CatalogApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task GetCategories_ReturnsCatalogData()
    {
        var response = await client.GetAsync("/api/v1/categories?isActive=true");
        var categories = await response.Content.ReadFromJsonAsync<List<CategoryResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(categories!);
        Assert.Equal("Categoría de prueba", categories![0].Description);
    }

    [Fact]
    public async Task GetUnknownProduct_ReturnsNotFound()
    {
        var response = await client.GetAsync("/api/v1/products/999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PublicCatalog_WithoutToken_RemainsAvailable()
    {
        var response = await client.GetAsync("/api/v1/categories?isActive=true");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("Customer", HttpStatusCode.Forbidden)]
    [InlineData("Administrator", HttpStatusCode.NoContent)]
    public async Task AdministratorPolicy_ValidatesBearerRole(string? role, HttpStatusCode expectedStatus)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/test/administrator-access");
        if (role is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(role));
        }

        var response = await client.SendAsync(request);

        Assert.Equal(expectedStatus, response.StatusCode);
    }

    private static string CreateToken(string role)
    {
        var token = new JwtSecurityToken(
            "Ecommerce.Identity",
            "Ecommerce.Services",
            [new Claim(JwtRegisteredClaimNames.Sub, "1"), new Claim("role", role)],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
                SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

[ApiController]
public sealed class AuthorizationProbeController : ControllerBase
{
    [Authorize(Policy = "AdministratorOnly")]
    [HttpGet("/test/administrator-access")]
    public IActionResult Get() => NoContent();
}

public sealed class CatalogApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:CatalogDatabase", "Server=test;");
        builder.UseSetting("Jwt:SigningKey", "test-signing-key-with-at-least-32-characters");
        builder.ConfigureServices(services =>
        {
            services.AddControllers().AddApplicationPart(typeof(AuthorizationProbeController).Assembly);
            services.RemoveAll<ICatalogRepository>();
            services.AddSingleton<ICatalogRepository, StubCatalogRepository>();
        });
    }

    private sealed class StubCatalogRepository : ICatalogRepository
    {
        public Task<IReadOnlyList<Category>> GetCategoriesAsync(bool? isActive, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Category>>([new Category(1, "Categoría de prueba", true)]);

        public Task<IReadOnlyList<Brand>> GetBrandsAsync(int? categoryId, bool? isActive, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Brand>>([new Brand(1, "Marca de prueba", true)]);

        public Task<IReadOnlyList<Product>> GetProductsAsync(int? categoryId, int? brandId, bool? isActive, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Product>>([]);

        public Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult<Product?>(null);

        public Task<bool> CanConnectAsync(CancellationToken cancellationToken) => Task.FromResult(true);
    }
}
