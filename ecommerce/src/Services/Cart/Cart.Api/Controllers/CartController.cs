using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Cart.Application;
using Cart.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Cart.Api.Controllers;

[ApiController]
[Authorize(Policy = "CustomerOnly")]
[Route("api/v1/cart")]
public sealed class CartController(CartService cartService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<CartSnapshot>(StatusCodes.Status200OK)]
    public async Task<ActionResult<CartSnapshot>> Get(CancellationToken cancellationToken)
    {
        return Ok(await cartService.GetAsync(GetCustomerId(), cancellationToken));
    }

    [HttpPost("items/{productId:int}")]
    public async Task<IActionResult> Add(int productId, CancellationToken cancellationToken)
    {
        var result = await cartService.AddAsync(GetCustomerId(), productId, cancellationToken);
        return result.Success
            ? NoContent()
            : Conflict(new ProblemDetails { Title = result.Message, Status = StatusCodes.Status409Conflict });
    }

    [HttpPatch("items/{productId:int}")]
    public async Task<IActionResult> ChangeQuantity(
        int productId,
        ChangeQuantityRequest request,
        CancellationToken cancellationToken)
    {
        var result = await cartService.ChangeQuantityAsync(
            GetCustomerId(), productId, request.Increase, cancellationToken);
        return result.Success
            ? NoContent()
            : BadRequest(new ProblemDetails { Title = result.Message, Status = StatusCodes.Status400BadRequest });
    }

    [HttpDelete("items/{productId:int}")]
    public async Task<IActionResult> Remove(int productId, CancellationToken cancellationToken)
    {
        return await cartService.RemoveAsync(GetCustomerId(), productId, cancellationToken)
            ? NoContent()
            : NotFound();
    }

    private int GetCustomerId()
    {
        var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                      User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(subject, out var customerId) || customerId <= 0)
            throw new InvalidOperationException("El token no contiene una identidad de cliente válida.");
        return customerId;
    }
}

public sealed record ChangeQuantityRequest(bool Increase);
