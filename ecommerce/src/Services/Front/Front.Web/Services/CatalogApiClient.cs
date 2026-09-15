using System.Net.Http.Json;
using Front.Web.Models;

namespace Front.Web.Services;

public sealed class CatalogApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<CategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken) =>
        await httpClient.GetFromJsonAsync<List<CategoryDto>>("api/v1/categories?isActive=true", cancellationToken) ?? [];

    public async Task<IReadOnlyList<BrandDto>> GetBrandsAsync(int? categoryId, CancellationToken cancellationToken)
    {
        var url = $"api/v1/brands?isActive=true{(categoryId.HasValue ? $"&categoryId={categoryId}" : string.Empty)}";
        return await httpClient.GetFromJsonAsync<List<BrandDto>>(url, cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsAsync(int? categoryId, int? brandId, CancellationToken cancellationToken)
    {
        var query = new List<string> { "isActive=true" };
        if (categoryId.HasValue) query.Add($"categoryId={categoryId}");
        if (brandId.HasValue) query.Add($"brandId={brandId}");
        return await httpClient.GetFromJsonAsync<List<ProductDto>>($"api/v1/products?{string.Join('&', query)}", cancellationToken) ?? [];
    }

    public async Task<ProductDto?> GetProductAsync(int id, CancellationToken cancellationToken)
    {
        var response = await httpClient.GetAsync($"api/v1/products/{id}", cancellationToken);
        return response.IsSuccessStatusCode ? await response.Content.ReadFromJsonAsync<ProductDto>(cancellationToken) : null;
    }
}
