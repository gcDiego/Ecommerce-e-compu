using Catalog.Application;
using Catalog.Domain;

namespace Catalog.UnitTests;

public sealed class CatalogServiceTests
{
    [Fact]
    public async Task GetProductByIdAsync_ReturnsRepositoryProduct()
    {
        var expected = new Product(
            7,
            "Producto",
            "Descripción",
            new Brand(2, "Marca", true),
            new Category(3, "Categoría", true),
            99.90m,
            4,
            null,
            null,
            true);
        var service = new CatalogService(new StubCatalogRepository(expected));

        var actual = await service.GetProductByIdAsync(7, CancellationToken.None);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetProductsAsync_ForwardsFiltersToRepository()
    {
        var repository = new StubCatalogRepository(null);
        var service = new CatalogService(repository);

        await service.GetProductsAsync(3, 2, true, CancellationToken.None);

        Assert.Equal((3, 2, true), repository.ProductFilters);
    }

    private sealed class StubCatalogRepository(Product? product) : ICatalogRepository
    {
        public (int? CategoryId, int? BrandId, bool? IsActive) ProductFilters { get; private set; }

        public Task<IReadOnlyList<Category>> GetCategoriesAsync(bool? isActive, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Category>>([]);

        public Task<IReadOnlyList<Brand>> GetBrandsAsync(int? categoryId, bool? isActive, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<Brand>>([]);

        public Task<IReadOnlyList<Product>> GetProductsAsync(int? categoryId, int? brandId, bool? isActive, CancellationToken cancellationToken)
        {
            ProductFilters = (categoryId, brandId, isActive);
            return Task.FromResult<IReadOnlyList<Product>>([]);
        }

        public Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken) =>
            Task.FromResult(product);

        public Task<bool> CanConnectAsync(CancellationToken cancellationToken) => Task.FromResult(true);
    }
}
