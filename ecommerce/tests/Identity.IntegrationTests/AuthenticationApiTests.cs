using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Identity.Application;
using Identity.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Identity.IntegrationTests;

public sealed class AuthenticationApiTests : IClassFixture<IdentityApiFactory>
{
    private readonly HttpClient client;

    public AuthenticationApiTests(IdentityApiFactory factory) => client = factory.CreateClient();

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "customer@test.com",
            password = "correcta",
            accountType = "Customer"
        });
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("accessToken", body);
        Assert.DoesNotContain("passwordHash", body);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "customer@test.com",
            password = "incorrecta",
            accountType = "Customer"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithoutToken_ReturnsUnauthorized()
    {
        var response = await client.PostAsJsonAsync("/api/v1/auth/change-password", new
        {
            currentPassword = "correcta",
            newPassword = "Nueva-Clave-123!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangePassword_WithValidToken_ReturnsNoContent()
    {
        var login = await client.PostAsJsonAsync("/api/v1/auth/login", new
        {
            email = "customer@test.com",
            password = "correcta",
            accountType = "Customer"
        });
        using var document = JsonDocument.Parse(await login.Content.ReadAsStringAsync());
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer", document.RootElement.GetProperty("accessToken").GetString());

        var response = await client.PostAsJsonAsync("/api/v1/auth/change-password", new
        {
            currentPassword = "correcta",
            newPassword = "Nueva-Clave-123!"
        });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}

public sealed class IdentityApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:IdentityDatabase", "Server=test;");
        builder.UseSetting("Jwt:SigningKey", "test-signing-key-with-at-least-32-characters");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IIdentityRepository>();
            services.RemoveAll<IPasswordVerifier>();
            services.AddSingleton<IIdentityRepository, StubRepository>();
            services.AddSingleton<IPasswordVerifier, StubVerifier>();
        });
    }

    private sealed class StubRepository : IIdentityRepository
    {
        public Task<IdentityAccount?> FindByEmailAsync(string email, AccountType accountType, CancellationToken cancellationToken) =>
            Task.FromResult<IdentityAccount?>(new(1, "Cliente", "Prueba", email, "hash", false, true, accountType));
        public Task<IdentityAccount?> FindByIdAsync(int id, AccountType accountType, CancellationToken cancellationToken) =>
            Task.FromResult<IdentityAccount?>(new(id, "Cliente", "Prueba", "customer@test.com", "hash", false, true, accountType));
        public Task<bool> UpdatePasswordHashAsync(int id, AccountType accountType, string expectedCurrentHash, string newHash, CancellationToken cancellationToken) => Task.FromResult(true);
        public Task<bool> CanConnectAsync(CancellationToken cancellationToken) => Task.FromResult(true);
    }

    private sealed class StubVerifier : IPasswordVerifier
    {
        public PasswordVerificationOutcome Verify(string password, string storedHash) =>
            password == "correcta" ? PasswordVerificationOutcome.Success : PasswordVerificationOutcome.Failed;
        public string Hash(string password) => "adaptive-hash";
    }
}
