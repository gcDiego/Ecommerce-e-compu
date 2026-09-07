namespace Catalog.Domain;

public sealed record Product(
    int Id,
    string Name,
    string Description,
    Brand Brand,
    Category Category,
    decimal Price,
    int Stock,
    string? ImagePath,
    string? ImageName,
    bool IsActive);
