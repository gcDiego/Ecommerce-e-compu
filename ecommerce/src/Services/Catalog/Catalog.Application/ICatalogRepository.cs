using Catalog.Domain;

namespace Catalog.Application;

public interface ICatalogRepository
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync(bool? isActive, CancellationToken cancellationToken);
    Task<IReadOnlyList<Brand>> GetBrandsAsync(int? categoryId, bool? isActive, CancellationToken cancellationToken);
    Task<IReadOnlyList<Product>> GetProductsAsync(int? categoryId, int? brandId, bool? isActive, CancellationToken cancellationToken);
    Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> CanConnectAsync(CancellationToken cancellationToken);
}
