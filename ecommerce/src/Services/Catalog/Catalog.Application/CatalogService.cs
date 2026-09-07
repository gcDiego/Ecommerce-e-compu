using Catalog.Domain;

namespace Catalog.Application;

public sealed class CatalogService(ICatalogRepository repository)
{
    public Task<IReadOnlyList<Category>> GetCategoriesAsync(bool? isActive, CancellationToken cancellationToken) =>
        repository.GetCategoriesAsync(isActive, cancellationToken);

    public Task<IReadOnlyList<Brand>> GetBrandsAsync(int? categoryId, bool? isActive, CancellationToken cancellationToken) =>
        repository.GetBrandsAsync(categoryId, isActive, cancellationToken);

    public Task<IReadOnlyList<Product>> GetProductsAsync(int? categoryId, int? brandId, bool? isActive, CancellationToken cancellationToken) =>
        repository.GetProductsAsync(categoryId, brandId, isActive, cancellationToken);

    public Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken) =>
        repository.GetProductByIdAsync(id, cancellationToken);
}
