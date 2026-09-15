using System.Net.Http.Headers;
using System.Net.Http.Json;
using Front.Web.Models;

namespace Front.Web.Services;

public sealed class CartApiClient(HttpClient httpClient)
{
    public async Task<CartSnapshotDto?> GetAsync(string token, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Get, "api/v1/cart", token);
        var response = await httpClient.SendAsync(request, cancellationToken);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<CartSnapshotDto>(cancellationToken) : null;
    }

    public Task<HttpResponseMessage> AddAsync(int productId, string token, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Post, $"api/v1/cart/items/{productId}", token, null, cancellationToken);

    public Task<HttpResponseMessage> ChangeAsync(int productId, bool increase, string token, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Patch, $"api/v1/cart/items/{productId}", token, JsonContent.Create(new { increase }), cancellationToken);

    public Task<HttpResponseMessage> RemoveAsync(int productId, string token, CancellationToken cancellationToken) =>
        SendAsync(HttpMethod.Delete, $"api/v1/cart/items/{productId}", token, null, cancellationToken);

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string url, string token, HttpContent? content, CancellationToken cancellationToken)
    {
        using var request = CreateRequest(method, url, token);
        request.Content = content;
        return await httpClient.SendAsync(request, cancellationToken);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, string token)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }
}
