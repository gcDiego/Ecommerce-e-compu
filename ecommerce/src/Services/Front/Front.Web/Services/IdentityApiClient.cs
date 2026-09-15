using System.Net.Http.Json;
using Front.Web.Models;

namespace Front.Web.Services;

public sealed class IdentityApiClient(HttpClient httpClient)
{
    public async Task<LoginResponseDto?> LoginAsync(string email, string password, CancellationToken cancellationToken)
    {
        var response = await httpClient.PostAsJsonAsync("api/v1/auth/login", new { email, password, accountType = "Customer" }, cancellationToken);
        return response.IsSuccessStatusCode
            ? await response.Content.ReadFromJsonAsync<LoginResponseDto>(cancellationToken)
            : null;
    }
}
