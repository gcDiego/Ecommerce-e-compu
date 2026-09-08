using System.ComponentModel.DataAnnotations;
using Identity.Application;
using Identity.Domain;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthenticationController(
    AuthenticationService authenticationService,
    PasswordChangeService passwordChangeService) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authenticationService.AuthenticateAsync(
            request.Email, request.Password, request.AccountType, cancellationToken);
        if (result is null)
            return Unauthorized(new ProblemDetails { Title = "Credenciales inválidas.", Status = StatusCodes.Status401Unauthorized });

        return Ok(new LoginResponse(
            result.Id, result.FirstName, result.LastName, result.Email,
            result.AccountType, result.MustResetPassword, result.AccessToken, result.ExpiresAt));
    }

    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangePassword(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var subject = User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                      User.FindFirstValue(ClaimTypes.NameIdentifier);
        var role = User.FindFirstValue("role") ?? User.FindFirstValue(ClaimTypes.Role);
        if (!int.TryParse(subject, out var accountId) ||
            !Enum.TryParse<AccountType>(role, true, out var accountType))
        {
            return Unauthorized();
        }

        var outcome = await passwordChangeService.ChangeAsync(
            accountId, accountType, request.CurrentPassword, request.NewPassword, cancellationToken);

        return outcome switch
        {
            PasswordChangeOutcome.Success => NoContent(),
            PasswordChangeOutcome.InvalidCredentials => Unauthorized(
                new ProblemDetails { Title = "La contraseña actual no es válida.", Status = StatusCodes.Status401Unauthorized }),
            PasswordChangeOutcome.Conflict => Conflict(
                new ProblemDetails { Title = "La contraseña cambió durante la solicitud. Intenta nuevamente.", Status = StatusCodes.Status409Conflict }),
            PasswordChangeOutcome.PasswordReused => BadRequest(
                new ProblemDetails { Title = "La nueva contraseña debe ser diferente de la actual.", Status = StatusCodes.Status400BadRequest }),
            _ => BadRequest(
                new ProblemDetails { Title = "La nueva contraseña no cumple la política requerida.", Status = StatusCodes.Status400BadRequest })
        };
    }
}

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password,
    AccountType AccountType);

public sealed record LoginResponse(
    int Id,
    string FirstName,
    string LastName,
    string Email,
    AccountType AccountType,
    bool MustResetPassword,
    string AccessToken,
    DateTimeOffset ExpiresAt);

public sealed record ChangePasswordRequest(
    [Required] string CurrentPassword,
    [Required, StringLength(128, MinimumLength = 12)] string NewPassword);
