using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Cart.Application;
using Cart.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace Cart.IntegrationTests;

public sealed class CartApiTests : IClassFixture<CartApiFactory>
{
    private const string SigningKey = "test-signing-key-with-at-least-32-characters";
    private readonly HttpClient client;

    public CartApiTests(CartApiFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task GetCart_WithoutToken_ReturnsUnauthorized()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/v1/cart")).StatusCode);
    }

    [Fact]
    public async Task GetCart_WithAdministratorToken_ReturnsForbidden()
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(3, "Administrator"));

        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/v1/cart")).StatusCode);
    }

    [Fact]
    public async Task GetCart_WithCustomerToken_UsesSubjectAndReturnsServerValues()
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(27, "Customer"));

        var response = await client.GetAsync("/api/v1/cart");
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(27, CartApiFactory.Repository.LastCustomerId);
        Assert.Equal(40m, json.RootElement.GetProperty("total").GetDecimal());
    }

    private static string CreateToken(int subject, string role)
    {
        var token = new JwtSecurityToken(
            "Ecommerce.Identity",
            "Ecommerce.Services",
            [new Claim(JwtRegisteredClaimNames.Sub, subject.ToString()), new Claim("role", role)],
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
                SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public sealed class CartApiFactory : WebApplicationFactory<Program>
{
    public static StubCartRepository Repository { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:CartDatabase", "Server=test;");
        builder.UseSetting("Jwt:SigningKey", "test-signing-key-with-at-least-32-characters");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<ICartRepository>();
            services.AddSingleton<ICartRepository>(Repository);
        });
    }

    public sealed class StubCartRepository : ICartRepository
    {
        public int LastCustomerId { get; private set; }

        public Task<IReadOnlyList<CartItem>> GetItemsAsync(int customerId, CancellationToken cancellationToken)
        {
            LastCustomerId = customerId;
            return Task.FromResult<IReadOnlyList<CartItem>>([new CartItem(8, "Producto", "Marca", 20m, 2)]);
        }

        public Task<bool> ContainsAsync(int customerId, int productId, CancellationToken cancellationToken) => Task.FromResult(false);
        public Task<CartOperationResult> ChangeQuantityAsync(int customerId, int productId, bool increase, CancellationToken cancellationToken) => Task.FromResult(new CartOperationResult(true, string.Empty));
        public Task<bool> RemoveAsync(int customerId, int productId, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken) => Task.FromResult(true);
    }
}