using Catalog.Application;
using Catalog.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.Api.Controllers;

[ApiController]
[Route("api/v1")]
public sealed class CatalogController(CatalogService catalogService) : ControllerBase
{
    [HttpGet("categories")]
    [ProducesResponseType<IReadOnlyList<Category>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Category>>> GetCategories(
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken) =>
        Ok(await catalogService.GetCategoriesAsync(isActive, cancellationToken));

    [HttpGet("brands")]
    [ProducesResponseType<IReadOnlyList<Brand>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Brand>>> GetBrands(
        [FromQuery] int? categoryId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken) =>
        Ok(await catalogService.GetBrandsAsync(categoryId, isActive, cancellationToken));

    [HttpGet("products")]
    [ProducesResponseType<IReadOnlyList<Product>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Product>>> GetProducts(
        [FromQuery] int? categoryId,
        [FromQuery] int? brandId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken) =>
        Ok(await catalogService.GetProductsAsync(categoryId, brandId, isActive, cancellationToken));

    [HttpGet("products/{id:int}")]
    [ProducesResponseType<Product>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Product>> GetProductById(int id, CancellationToken cancellationToken)
    {
        var product = await catalogService.GetProductByIdAsync(id, cancellationToken);
        return product is null ? NotFound() : Ok(product);
    }
}
