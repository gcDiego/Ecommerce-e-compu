using Catalog.Api.Models;
using Catalog.Application;
using Catalog.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class CatalogController(CatalogService catalogService) : ControllerBase
{
    [HttpGet("categories")]
    [ProducesResponseType<IReadOnlyList<CategoryResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetCategories(
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var categories = await catalogService.GetCategoriesAsync(isActive, cancellationToken);
        return Ok(categories.Select(c => new CategoryResponse(c.Id, c.Description, c.IsActive)).ToList());
    }

    [HttpGet("brands")]
    [ProducesResponseType<IReadOnlyList<BrandResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BrandResponse>>> GetBrands(
        [FromQuery] int? categoryId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var brands = await catalogService.GetBrandsAsync(categoryId, isActive, cancellationToken);
        return Ok(brands.Select(b => new BrandResponse(b.Id, b.Description, b.IsActive)).ToList());
    }

    [HttpGet("products")]
    [ProducesResponseType<IReadOnlyList<ProductResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetProducts(
        [FromQuery] int? categoryId,
        [FromQuery] int? brandId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var products = await catalogService.GetProductsAsync(categoryId, brandId, isActive, cancellationToken);
        return Ok(products.Select(MapProduct).ToList());
    }

    [HttpGet("products/{id:int}")]
    [ProducesResponseType<ProductResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProductById(int id, CancellationToken cancellationToken)
    {
        var product = await catalogService.GetProductByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(MapProduct(product));
    }

    private static ProductResponse MapProduct(Product p) =>
        new(
            p.Id,
            p.Name,
            p.Description,
            new BrandResponse(p.Brand.Id, p.Brand.Description, p.Brand.IsActive),
            new CategoryResponse(p.Category.Id, p.Category.Description, p.Category.IsActive),
            p.Price,
            p.Stock,
            p.IsActive);
}
