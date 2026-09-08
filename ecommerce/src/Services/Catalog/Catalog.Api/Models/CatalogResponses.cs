namespace Catalog.Api.Models;

public sealed record CategoryResponse(int Id, string Description, bool IsActive);

public sealed record BrandResponse(int Id, string Description, bool IsActive);

public sealed record ProductResponse(
    int Id,
    string Name,
    string Description,
    BrandResponse Brand,
    CategoryResponse Category,
    decimal Price,
    int Stock,
    bool IsActive);
